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
    private int pelaajanMassa = 1;
    private int pelaajanLeveys = 70;
    private int pelaajanKorkeus = 140;
    private int kameranNopeus = 10;
    const int TAHDEN_LEVEYS = 40;
    const int TAHDEN_KORKEUS = 40;
    const int LIIKUTUS_VEKTORI = 200;
    const int PILVEN_LEVEYS = 200;
    const int PILVEN_KORKEUS = 250;

    private IntMeter pelaajanPisteet;  

    /// <summary>
    /// Pääohjelma suorittaa pelin aloituksen ja luo hahmot ym
    /// </summary>
    public override void Begin()
    {
        PhysicsObject pelaaja = LuoKentta();
        SetWindowSize(1920, 1080);
        LiikutaKenttaa(pelaaja);
        Myrskypilvi();
        LuoTahti();
        LisaaNappaimet(pelaaja);
        LisaaPisteLaskuri();

        Timer liikutusAjastin = new Timer();
        liikutusAjastin.Interval = 0.01;
        liikutusAjastin.Timeout += delegate () 
        { 
            SiirraPelaajaaOikealle(pelaaja); 
        };
        liikutusAjastin.Start();
        
        AddCollisionHandler<PhysicsObject, Tahti>(pelaaja, TormaaTahteen);
        AddCollisionHandler<PhysicsObject, Pilvi>(pelaaja, TormaaPilveen);
        AddCollisionHandler(pelaaja, "puu", TormaaKuolettavaan);
        
    }


    /// <summary>
    /// Liikuttaa kameraa eteenpäin
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void LiikutaKenttaa(PhysicsObject pelaaja)
    { 
        Camera.X = Level.Left + 100;

        Camera.Velocity = new Vector(150, 0);
        pelaaja.Push(new Vector(kameranNopeus, 0.0));

    }


    /// <summary>
    /// Määritellään, mitä pelaajalle tapahtuu, kun näppäimiä painetaan
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    /// <param name="vektori">Suunta, johon liikutaan</param>
    public void LiikutaPelaajaa(PhysicsObject pelaaja, Vector vektori)
    {  
        pelaaja.Push(vektori);

    }
    

    /// <summary>
    /// Pannaan ohjelma kuuntelemaan näppäimistöä ja määritellään pelin ohjaimet
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void LisaaNappaimet(PhysicsObject pelaaja)
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa vasemmalle", pelaaja, new Vector(-LIIKUTUS_VEKTORI, 0));
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa oikealle", pelaaja, new Vector(LIIKUTUS_VEKTORI, 0));
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, LIIKUTUS_VEKTORI));
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -LIIKUTUS_VEKTORI));

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

    }


    /// <summary>
    /// Yhdistää pelaajan pisteet pistelaskuriin, sekä määrittelee sen paikan
    /// </summary>
    public void LisaaPisteLaskuri()
    {
        pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100, Screen.Bottom + 100);

    }

    /// <summary>
    /// Lisää peliin puita
    /// </summary>
    public void LisaaPuu()
    {
        for (int i = 0; i < 30; i++)
        {
            PhysicsObject puu = new PhysicsObject(200, 500);
            puu.X = RandomGen.NextInt(-9000, 9000);
            puu.Bottom = Level.Bottom;
            puu.IgnoresGravity = true;
            puu.CanRotate = false;
            puu.Image = LoadImage("puu400korkea");
            puu.IgnoresCollisionResponse = true;
            puu.Tag = "puu";
            Add(puu);

        }

    }


    /// <summary>
    /// Aliohjelma määrittelee pelikentän ja asettaa sille reunat
    /// </summary>
    public PhysicsObject LuoKentta()
    {
        double[] korkeus = { 3, 3 };

        Level.Width = 20000;
        Level.Height = 1080;
        Level.CreateBorders();

        LisaaPuu();

        Gravity = new Vector(0, -80);

        PhysicsObject maanpinta = Level.CreateGround(korkeus, 30);
        maanpinta.Color = Color.Green;
        maanpinta.Tag = "maa";
        //Level.Background.MovesWithCamera = true;
        Level.Background.Image = LoadImage("leijaTaustaKuva");
        Level.Background.TileToLevel();

        PhysicsObject pelaaja = LuoPelaaja(pelaajanLeveys, pelaajanKorkeus);
        return pelaaja;

    }


    /// <summary>
    /// Aliohjelma, joka määrittelee pelaajan pelihahmon, sen ominaisuudet ja piirtää sen haluttuun paikkaan
    /// </summary>
    /// <param name="leveys">Pelihahmon leveys</param>
    /// <param name="korkeus">Pelihahmon korkeus</param>
    public PhysicsObject LuoPelaaja(double leveys, double korkeus)
    {
        Vector pelaajanPaikkaAlussa = new Vector(Level.Left + 250, 50);
        PhysicsObject pelaaja = new PhysicsObject(leveys, korkeus);
        pelaaja.Mass = 1;
        pelaaja.Position = pelaajanPaikkaAlussa;
        pelaaja.Image = LoadImage ("leija240korkea");
        pelaaja.CanRotate = false;
        AddCollisionHandler(pelaaja, "maa", TormaaKuolettavaan);

        Add(pelaaja);
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
        for (int i = 0; i <= 20; i++)
        {
            Tahti tahti = new Tahti(TAHDEN_LEVEYS, TAHDEN_KORKEUS);
            tahti.Image = LoadImage("tahdenKuva");
            tahti.Mass = pelaajanMassa / 100;
            tahti.Position = RandomGen.NextVector(Level.BoundingRect);
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
    public void Myrskypilvi()
    {
        for (int i = 0; i < 7; i++)
        {
            Pilvi pilvi = new Pilvi(PILVEN_LEVEYS, PILVEN_KORKEUS);
            pilvi.Position = RandomGen.NextVector(Level.BoundingRect);
            pilvi.IgnoresGravity = true;
            pilvi.CanRotate = false;
            pilvi.Image = LoadImage ("myrskypilvi400korkea");
            pilvi.Tag = "pilvi";
            pilvi.IgnoresCollisionResponse = true;
            Add(pilvi);

        }

    }


    public void SiirraPelaajaaOikealle(PhysicsObject pelaaja)
    {
        pelaaja.Push(new Vector(kameranNopeus, 0));
    }


    /// <summary>
    /// Aliohjelma kertoo, mitä tapahtuu, kun pelaaja törmää johonkin kuolettavaan
    /// </summary>
    /// <param name="pelaaja">Pelihahmo</param>
    /// <param name="kohde">Maa, johon törmätään</param>
    public void TormaaKuolettavaan(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        Gravity = new Vector(0, -800);
        StopAll();
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Down);
        MessageDisplay.Add("Kuolit, peli loppui!");
        MessageDisplay.Add("Keräsit " + pelaajanPisteet.ToString() + " pistettä.");
        pelaaja.Image = LoadImage ("leija240korkeaKuoli");

    }


    /// <summary>
    /// Pilveen törmääminen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    /// <param name="kohde">Kohde, johon törmätään</param>
    public void TormaaPilveen(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        Gravity = new Vector(0, -800);
        StopAll();
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Down);
        MessageDisplay.Add("Kuolit, peli loppui!");
        SoundEffect ukkosenAani = LoadSoundEffect("ukkosenJyrina.wav");
        ukkosenAani.Play();
        pelaaja.Image = LoadImage("leija240korkeaKuoli");

    }

    /// <summary>
    /// Tähteen törmääminen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, joka törmää</param>
    /// <param name="tahti">Tähti, johon törmätään</param>
    public void TormaaTahteen(PhysicsObject pelaaja, Tahti tahti)
    {
        tahti.TuhoaTahti();
        pelaajanPisteet.AddValue(1);
        MessageDisplay.Add("Keräsit tähden");
        SoundEffect keraaTahtiAani = LoadSoundEffect("keraaTahtiAani.wav");
        keraaTahtiAani.Play();
        
    }

}

