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
    private int kameranNopeus = 80;
    const int KENTAN_LEVEYS = 10000;
    const int TAHDEN_LEVEYS = 40;
    const int TAHDEN_KORKEUS = 40;
    const int LIIKUTUS_VEKTORI = 200;
    const int PILVEN_LEVEYS = 200;
    const int PILVEN_KORKEUS = 250;
    const int VAROETAISYYS = 600;

    private readonly Image pilvenKuva = LoadImage("myrskypilvi400korkea");
    private readonly Image puunKuva = LoadImage("puu400korkea");
    private readonly Image tahdenKuva = LoadImage("tahdenKuva");
    private readonly SoundEffect ukkosenAani = LoadSoundEffect("ukkosenJyrina.wav");
    private readonly SoundEffect keraaTahtiAani = LoadSoundEffect("keraaTahtiAani.wav");

    private IntMeter pelaajanPisteet;
    private bool peliKaynnissa = false;


    /// <summary>
    /// Aloittaa pelin
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(1920, 1080);
        
        PhysicsObject pelaaja = LuoPelaaja(70, 140);
        LuoKentta(1080);
        Camera.X = Level.Left + 100;
        LisaaPuu(pelaaja.Position);
        Myrskypilvi(pelaaja.Position);
        LuoTahti();
        LisaaNappaimet(pelaaja);
        //LuoVihollinen(pelaaja.Position);
        
        pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100, Screen.Bottom + 100);


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
        AddCollisionHandler(pelaaja, "pilvi", TormaaPilveen);
        AddCollisionHandler(pelaaja, "vihollinen", TormaaKuolettavaan);

        peliKaynnissa = true;

    }


    /// <summary>
    /// Liikuttaa pelaajaa kameran mukana, jotta pelaaja ei kameran liikkuessa nopeammin jäisi jälkeen
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
        kameranNopeus += 3;
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


    /// <summary>
    /// Lisää peliin puita
    /// </summary>
    public void LisaaPuu(Vector pelaajanSijainti)
    {
        for (int i = 0; i < 50; i++)
        {
            double puunLeveys = RandomGen.NextInt(50, 250);
            PhysicsObject puu = new PhysicsObject(puunLeveys, 2.5 * puunLeveys);
            puu.Y = Level.Bottom;
            puu.Bottom = Level.Bottom;
            puu.IgnoresGravity = true;
            puu.CanRotate = false;
            puu.Image = puunKuva;
            puu.IgnoresCollisionResponse = true;
            puu.Tag = "vihollinen";

            // Arvotaan puun x niin, ettei se tule liian lähelle pelaajaa
            Vector puunSijainti;
            do
            {
                puunSijainti = new Vector(RandomGen.NextDouble(Level.Left, Level.Right), puu.Y);
            } while (Vector.Distance(puunSijainti, pelaajanSijainti) < VAROETAISYYS -100);
            puu.Position = puunSijainti;
            Add(puu);
        }
    }


    /// <summary>
    /// Aliohjelma määrittelee pelikentän ja asettaa sille reunat
    /// </summary>
    public void LuoKentta(int kentanKorkeus)
    {
        double[] korkeus = { 3, 3 };
        Level.Width = KENTAN_LEVEYS;
        Level.Height = kentanKorkeus;
        Level.CreateBorders();
        Gravity = new Vector(0, -80);
        PhysicsObject maanpinta = Level.CreateGround(korkeus, 30);
        maanpinta.Color = Color.Green;
        maanpinta.Tag = "vihollinen";
        Level.Background.Image = LoadImage("leijaTaustaKuva");
        Level.Background.TileToLevel();    
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
    public IntMeter LuoPisteLaskuri(double x, double y)
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

    //public void LuoVihollinen(Vector pelaajanSijainti, int montakoLuodaan)
    //{


    //}


    /// <summary>
    /// Aliohjelma määrittelee peliin myrskypilven ja sen ominaisuudet
    /// </summary>
    public void Myrskypilvi(Vector pelaajanSijainti)
    {
        for (int i = 0; i < 10; i++)
        {
            int y = RandomGen.NextInt(0, 1080 / 2);
            PhysicsObject pilvi = new PhysicsObject(PILVEN_LEVEYS, PILVEN_KORKEUS);
            //pilvi.Position = new Vector(x, y);
            pilvi.IgnoresGravity = true;
            pilvi.CanRotate = false;
            pilvi.Image = pilvenKuva;
            pilvi.Tag = "pilvi";
            pilvi.IgnoresCollisionResponse = true;

            // Katsotaan, ettei pilvi tule liian lähelle pelaajaa
            Vector pilvenSijainti;
            do
            {
                pilvenSijainti = new Vector(RandomGen.NextDouble(Level.Left, Level.Right), y);
            } while (Vector.Distance(pilvenSijainti, pelaajanSijainti) < VAROETAISYYS);
            pilvi.Position = pilvenSijainti;
            Add(pilvi);
        }
    }


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
            ukkosenAani.Play();
            pelaaja.Image = LoadImage("leija240korkeaKuoli");
            MessageDisplay.Add("Kuljettu matka " + KuljettuMatka(pelaaja.Position));
            MessageDisplay.Add("Keräsit tähtiä " + pelaajanPisteet +  " ja  sait yhteensä " + KokonaisPisteet(pelaaja.Position) + " pistettä.");
            ukkosenAani.Play();
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
            keraaTahtiAani.Play();
        }
    }


    /// <summary>
    /// Laskee pelaajan pisteet kuljetun matkan ja kerättyjen tähtien perusteella
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jolla pelataan</param>
    /// <returns>Pelin kokonaispisteet</returns>
    public double KokonaisPisteet(Vector pelaajanSijaintiKuollessa)
    {
        double kuljettuMatka = Vector.Distance(pelaajanPaikkaAlussa, pelaajanSijaintiKuollessa);
        double kokonaisPisteet;
      
        // Jokaisesta kuljetusta 500:sta saa pisteitä 50
        // Tutkitaan tässä, kuinka monta 500:n pätkää on kuljettu ja kerrotaan kertoimella
        // Lisätään siihen kerätyistä tähdistä saadut pisteet
        // = saadaan kokonaispisteet
        int i = 0;
        int kerroin = 0;
        while (i < kuljettuMatka)
        {
            if (i % 1000 == 0)
            {
                kerroin++;
                
            }
            i++;
        }
        kokonaisPisteet = pelaajanPisteet + kerroin * 10;
        return kokonaisPisteet;
    }

    public double KuljettuMatka(Vector pelaajanPaikkaKuollessa)
    {
        double kuljettuMatka = Vector.Distance(pelaajanPaikkaAlussa, pelaajanPaikkaKuollessa);
        return kuljettuMatka;
    }
}

