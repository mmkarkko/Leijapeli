using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

// HUOM:
// Tämä projekti käyttää hyvin kokeellista FarseerPhysics -fysiikkamoottoria
// Varmista että projektin paketit ovat aina uusimmassa versiossa.
// Katso myös https://tim.jyu.fi/view/kurssit/jypeli/farseer
// kaikista tämän version eroavaisuuksista sekä tunnetuista ongelmista. Täydennä listaa tarvittaessa.


///@author Miia Arkko
///@version 2.3.2021
/// <summary>
/// Peli, jossa lännetään leijalla, kerätään pisteitä ja varotaan osumasta muihin esineisiin.
/// </summary>
public class Leija : PhysicsGame
{
    Vector pelaajanPaikkaAlussa = new Vector(-4700, 50);
    private int pelaajanLeveys = 70;
    private int pelaajanKorkeus = 140;
    private int kameranNopeus = 80;
    const int KENTAN_LEVEYS = 10000;
    const int KENTAN_KORKEUS = 1080;
    const int TAHDEN_LEVEYS = 40;
    const int TAHDEN_KORKEUS = 40;
    const int LIIKUTUS_VEKTORI = 200;
    const int PILVEN_LEVEYS = 200;
    const int PILVEN_KORKEUS = 250;
    const int VAROETAISYYS = 600;

    private readonly Image pilvenKuva = LoadImage("myrskypilvi400korkea");
    private readonly Image puunKuva = LoadImage("puu400korkea");
    private readonly Image tahdenKuva = LoadImage("tahdenKuva");

    private IntMeter pelaajanPisteet;
    private bool peliKaynnissa = false;


    /// <summary>
    /// Aloittaa pelin
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(1920, 1080);

        PhysicsObject pelaaja = LuoKentta();
        LisaaPuu();
        Myrskypilvi(pelaaja.Position);
        LuoTahti();
        
        LiikutaKenttaa(kameranNopeus);
        LisaaNappaimet(pelaaja);
        pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100, Screen.Bottom + 100);
        Camera.X = Level.Left + 100;

        // Kameranliikutus ajastin, siirtää myös pelaajaa oikealle
        Timer liikutusAjastin = new Timer();
        liikutusAjastin.Interval = 0.5;
        liikutusAjastin.Timeout += KasvataKameranNopeutta;
        liikutusAjastin.Timeout += delegate () 
        {
            SiirraPelaajaaOikealle(pelaaja); 
        };
        liikutusAjastin.Start();

        AddCollisionHandler<PhysicsObject, Tahti>(pelaaja, TormaaTahteen);
        AddCollisionHandler<PhysicsObject, Pilvi>(pelaaja, TormaaPilveen);
        AddCollisionHandler(pelaaja, "vihollinen", TormaaKuolettavaan);

        peliKaynnissa = true;

        if (!peliKaynnissa) liikutusAjastin.Stop();

    }


    /// <summary>
    /// Liikuttaa pelaajaa kameran mukana
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void SiirraPelaajaaOikealle(PhysicsObject pelaaja)
    {
        pelaaja.Push(new Vector(kameranNopeus + 8, 0));

    }


    /// <summary>
    /// Kasvattaa kameran nopeutta vähitellen
    /// </summary>
    private void KasvataKameranNopeutta()
    {
        kameranNopeus = kameranNopeus + 3;
        LiikutaKenttaa(kameranNopeus);

    }


    /// <summary>
    /// Liikuttaa kameraa eteenpäin
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void LiikutaKenttaa(int kameranNopeusX)
    {
        Camera.Velocity = new Vector(kameranNopeusX, 0);

    }


    /// <summary>
    /// Määritellään, mitä pelaajalle tapahtuu, kun näppäimiä painetaan
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    /// <param name="vektori">Suunta, johon liikutaan</param>
    public void LiikutaPelaajaaNappaimilla(PhysicsObject pelaaja, Vector vektori)
    {  
        pelaaja.Push(vektori);

    }
    

    /// <summary>
    /// Pannaan ohjelma kuuntelemaan näppäimistöä ja määritellään pelin ohjaimet
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void LisaaNappaimet(PhysicsObject pelaaja)
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa vasemmalle", pelaaja, new Vector(-LIIKUTUS_VEKTORI, 0));
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa oikealle", pelaaja, new Vector(LIIKUTUS_VEKTORI, 0));
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, LIIKUTUS_VEKTORI));
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -LIIKUTUS_VEKTORI));

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    ///// <summary>
    ///// Yhdistää pelaajan pisteet pistelaskuriin, sekä määrittelee sen paikan
    ///// </summary>
    //public void LisaaPisteLaskuri()
    //{
    //    //pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100, Screen.Bottom + 100);

    //}

    /// <summary>
    /// Lisää peliin puita
    /// </summary>
    public void LisaaPuu()
    {
        for (int i = 0; i < 50; i++)
        {
            double x = RandomGen.NextDouble(Level.Left, Level.Right);
            double puunLeveys = RandomGen.NextInt(50, 250);
            PhysicsObject puu = new PhysicsObject(puunLeveys, 2.5 * puunLeveys);
            puu.X = x;
            puu.Bottom = Level.Bottom;
            puu.IgnoresGravity = true;
            puu.CanRotate = false;
            puu.Image = puunKuva;
            puu.IgnoresCollisionResponse = true;
            puu.Tag = "vihollinen";
            Add(puu);
        }
    }


    /// <summary>
    /// Aliohjelma määrittelee pelikentän ja asettaa sille reunat
    /// </summary>
    public PhysicsObject LuoKentta()
    {
        double[] korkeus = { 3, 3 };
        PhysicsObject pelaaja = LuoPelaaja(pelaajanLeveys, pelaajanKorkeus);

        Level.Width = KENTAN_LEVEYS;
        Level.Height = KENTAN_KORKEUS;
        Level.CreateBorders();

        Gravity = new Vector(0, -80);

        PhysicsObject maanpinta = Level.CreateGround(korkeus, 30);
        maanpinta.Color = Color.Green;
        maanpinta.Tag = "vihollinen";
        Level.Background.Image = LoadImage("leijaTaustaKuva");
        Level.Background.TileToLevel();    
        return pelaaja;
    }


    /// <summary>
    /// Aliohjelma, joka määrittelee pelaajan pelihahmon, sen ominaisuudet ja piirtää sen haluttuun paikkaan
    /// </summary>
    /// <param name="leveys">Pelihahmon leveys</param>
    /// <param name="korkeus">Pelihahmon korkeus</param>
    public PhysicsObject LuoPelaaja(double leveys, double korkeus)
    { 
        PhysicsObject pelaaja = new PhysicsObject(leveys, korkeus);
        pelaaja.Mass = 0.5;
        pelaaja.Position = pelaajanPaikkaAlussa;
        pelaaja.Image = LoadImage ("leija240korkea");
        pelaaja.CanRotate = false;      
        Add(pelaaja, 1);
        return pelaaja;
    }


    /// <summary>
    /// Luo peliin pistelaskurin näytön
    /// </summary>
    /// <param name="x">Pistelaskurin sijainti x-akselilla</param>
    /// <param name="y">Pistelaskurin sijainti y-akselilla</param>
    /// <returns>Pistelaskuri</returns>
    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);
        Label pisteNaytto = new Label(100, 100);
        pisteNaytto.BindTo(laskuri);
        pisteNaytto.X = x;
        pisteNaytto.Y = y;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.BorderColor = Color.White;
        pisteNaytto.Color = Color.White;
        Add(pisteNaytto);
        return laskuri;
    }


    /// <summary>
    /// Luodaan kentälle kerättävät tähdet
    /// </summary>
    public void LuoTahti()
    {
        for (int i = 0; i <= 100; i++)
        {
            int x = RandomGen.NextInt(-KENTAN_LEVEYS / 2, KENTAN_LEVEYS / 2);
            int y = RandomGen.NextInt(0, 500);
            Tahti tahti = new Tahti(TAHDEN_LEVEYS, TAHDEN_KORKEUS);
            tahti.Image = tahdenKuva;
            tahti.Position = new Vector(x, y);
            tahti.IgnoresCollisionResponse = true;
            tahti.IgnoresPhysicsLogics = true;
            tahti.IgnoresGravity = true;
            tahti.CanRotate = false; 
            Add(tahti);
        }
    }


    /// <summary>
    /// Aliohjelma määrittelee peliin myrskypilven ja sen ominaisuudet
    /// </summary>
    public void Myrskypilvi(Vector pelaajanSijainti)
    {
        for (int i = 0; i < 10; i++)
        {
            //int x = RandomGen.NextInt(-KENTAN_LEVEYS / 2, KENTAN_LEVEYS / 2);
            //int y = RandomGen.NextInt(0, KENTAN_KORKEUS / 2);
            Pilvi pilvi = new Pilvi(PILVEN_LEVEYS, PILVEN_KORKEUS);
            //pilvi.Position = new Vector(x, y);
            pilvi.IgnoresGravity = true;
            pilvi.CanRotate = false;
            pilvi.Image = pilvenKuva;
            pilvi.Tag = "vihollinen";
            pilvi.IgnoresCollisionResponse = true;
           
            // Katsotaan, ettei pilvi tule liian lähelle pelaajaa
            Vector pilvenSijainti;
            do
            {
                pilvenSijainti = RandomGen.NextVector(Level.BoundingRect);
            } while (Vector.Distance(pilvenSijainti, pelaajanSijainti) < VAROETAISYYS);

            pilvi.Position = pilvenSijainti;
            Add(pilvi);
        }
    }


    //public void SiirraPelaajaaOikealle(PhysicsObject pelaaja)
    //{
    //    pelaaja.Push(new Vector(kameranNopeus, 0));
    //}


    /// <summary>
    /// Aliohjelma kertoo, mitä tapahtuu, kun pelaaja törmää johonkin kuolettavaan
    /// </summary>
    /// <param name="pelaaja">Pelihahmo</param>
    /// <param name="kohde">Maa, johon törmätään</param>
    public void TormaaKuolettavaan(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        if (peliKaynnissa)
        {
            Gravity = new Vector(0, -800);
            StopAll();
            Keyboard.Disable(Key.Left);
            Keyboard.Disable(Key.Right);
            Keyboard.Disable(Key.Up);
            Keyboard.Disable(Key.Down);
            MessageDisplay.Add("Kuolit, peli loppui!");
            MessageDisplay.Add("Keräsit " + pelaajanPisteet.ToString() + " pistettä.");
            pelaaja.Image = LoadImage("leija240korkeaKuoli");

            peliKaynnissa = false;
        }
    }


    /// <summary>
    /// Pilveen törmääminen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    /// <param name="kohde">Kohde, johon törmätään</param>
    public void TormaaPilveen(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        if (peliKaynnissa)
        {
            Gravity = new Vector(0, -800);
            Keyboard.Disable(Key.Left);
            Keyboard.Disable(Key.Right);
            Keyboard.Disable(Key.Up);
            Keyboard.Disable(Key.Down);
            //MessageDisplay.Add("Keräsit " + pelaajanPisteet.ToString() + " pistettä.");
            SoundEffect ukkosenAani = LoadSoundEffect("ukkosenJyrina.wav");
            ukkosenAani.Play();
            pelaaja.Image = LoadImage("leija240korkeaKuoli");
            MessageDisplay.Add("Keräsit " + KokonaisPisteet(pelaaja) + " pistettä.");
            peliKaynnissa = false;
        }
    }

    /// <summary>
    /// Tähteen törmääminen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, joka törmää</param>
    /// <param name="tahti">Tähti, johon törmätään</param>
    public void TormaaTahteen(PhysicsObject pelaaja, Tahti tahti)
    {
        if (peliKaynnissa)
        {
            tahti.TuhoaTahti();
            pelaajanPisteet.AddValue(1);
            MessageDisplay.Add("Keräsit tähden");
            SoundEffect keraaTahtiAani = LoadSoundEffect("keraaTahtiAani.wav");
            keraaTahtiAani.Play();
        }
    }


    /// <summary>
    /// Laskee pelaajan pisteet kuljetun matkan ja kerättyjen tähtien perusteella
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jolla pelataan</param>
    /// <returns>Pelin kokonaispisteet</returns>
    public double KokonaisPisteet(PhysicsObject pelaaja)
    {
        double kuljettuMatka = Vector.Distance(pelaajanPaikkaAlussa, pelaaja.Position);
        double kokonaisPisteet;
        
        // Jokaisesta kuljetusta 500:sta saa pisteitä 50
        // Tutkitaan tässä, kuinka monta 500:n pätkää on kuljettu ja kerrotaan kertoimella
        // Lisätään siihen kerätyistä tähdistä saadut pisteet
        // = saadaan kokonaispisteet
        int i = 0;
        int kerroin = 0;
        while (i < kuljettuMatka)
        {
            if (i % 500 == 0)
            {
                kerroin++;
            }
            i++;
        }
        kokonaisPisteet = pelaajanPisteet + kerroin * 50;
        Convert.ToInt32(kokonaisPisteet);
        return kokonaisPisteet;
    }
}

