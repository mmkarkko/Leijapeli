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
///@version 8.2.2021
/// <summary>
/// Peli, jossa lännetään leijalla, kerätään pisteitä ja varotaan osumasta muihin esineisiin.
/// </summary>
public class Leija : PhysicsGame
{

    PhysicsObject pelaaja;
    private int pelaajanMassa = 1000;
    private int pelaajanLeveys = 70;
    private int pelaajanKorkeus = 140;
    private int kameranNopeus = 10;
    const int TAHDEN_LEVEYS = 40;
    const int TAHDEN_KORKEUS = 40;
    const int LIIKUTUS_VEKTORI = 50;

    private IntMeter pelaajanPisteet;
    Timer aikaLaskuri = new Timer();

    Image taustaKuva = LoadImage("leijaTaustakuva.jpg");
    Image pelaajanKuva = LoadImage("leija240korkea.png");
    Image pelaajaKuoliKuva = LoadImage("leija240korkeaKuoli.png");
    Image myrskypilvi = LoadImage("myrskypilvi400korkea.png");
    Image puunKuva = LoadImage("puu400korkea.png");
    Image tahdenKuva = LoadImage("tahdenKuva.png");
    SoundEffect ukkosenAani = LoadSoundEffect("ukkosenJyrina.wav");
    SoundEffect keraaTahtiAani = LoadSoundEffect("keraaTahtiAani.wav");


    /// <summary>
    /// Pääohjelma suorittaa pelin aloituksen ja kutsuu luomaan hahmot ja kentän ym
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(1920, 1080);
        LuoKentta();
        LiikutaKenttaa();
        //AikaLaskuri();

        Myrskypilvi(ArvoLuku(), ArvoLuku());
        LuoTahti();

        LisaaNappaimet();

        LisaaPisteLaskuri();

        //Camera.FollowX(pelaaja);

        Timer liikutusAjastin = new Timer();
        liikutusAjastin.Interval = 0.01;
        liikutusAjastin.Timeout += SiirraPelaajaaOikealle;
        liikutusAjastin.Start();

        AddCollisionHandler<PhysicsObject, Tahti>(pelaaja, TormaaTahteen);

        //double[,] sijainnit = new double[10, 2];
        //sijainnit[0, 0] = 100;

    }





    /// <summary>
    /// Funktio arpoo satunnaisen luvun
    /// </summary>
    /// <returns>Satunnainen luku</returns>
    public int ArvoLuku()
    {
        int luku = RandomGen.NextInt(-500, 600);
        return luku;

    }


    /// <summary>
    /// Laitetaan kamera siirtymään eteenpäin 
    /// </summary>
    public void LiikutaKenttaa()
    { 
        Camera.X = Level.Left + 100;

        Camera.Velocity = new Vector(150, 0);
        pelaaja.Push(new Vector(kameranNopeus, 0.0));


    }


    /// <summary>
    /// Määritellään, mitä pelaajalle tapahtuu, kun näppäimiä painetaan
    /// </summary>
    /// <param name="vektori">Suunta, johon liikutaan</param>
    public void LiikutaPelaajaa(PhysicsObject pelaaja, Vector vektori)
    {  
        pelaaja.Push(vektori);
        //if (pelaaja.X <= Camera.X - 300)
        //{ 
        //    pelaaja.Push(-vektori);
            

        //    //if (pelaaja.X >= Camera.X - 300) return;
        //}

    }
    


    /// <summary>
    /// Pannaan ohjelma kuuntelemaan näppäimistöä ja määritellään pelin ohjaimet
    /// </summary>
    public void LisaaNappaimet()
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa vasemmalle", pelaaja, new Vector(-LIIKUTUS_VEKTORI, 0));
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa oikealle", pelaaja, new Vector(LIIKUTUS_VEKTORI, 0));
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, LIIKUTUS_VEKTORI));
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaPelaajaa, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -LIIKUTUS_VEKTORI));

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Aliohjelma, joka määrittelee pelaajan pelihahmon, sen ominaisuudet ja piirtää sen haluttuun paikkaan
    /// </summary>
    /// <param name="leveys">Pelihahmon leveys</param>
    /// <param name="korkeus">Pelihahmon korkeus</param>
    public void LisaaPelaaja(double leveys, double korkeus)
    {
        Vector pelaajanPaikkaAlussa = new Vector(Level.Left + 250, 50);
        pelaaja = new PhysicsObject(leveys, korkeus);
        pelaaja.Mass = 1000;
        pelaaja.LinearDamping = 1;
        pelaaja.Restitution = 0.0;
        pelaaja.Position = pelaajanPaikkaAlussa;
        pelaaja.Image = pelaajanKuva;
        pelaaja.CanRotate = false;
        AddCollisionHandler(pelaaja, "maa", TormaaKuolettavaan);

        Add(pelaaja);

    }


    /// <summary>
    /// Yhdistää pelaajan pisteet pistelaskuriin, sekä määrittelee sen paikan
    /// </summary>
    public void LisaaPisteLaskuri()
    {
        pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100, Screen.Bottom + 100);

    }


    /// <summary>
    /// Määritellään peliin "vihollinen" puu
    /// </summary>
    /// <param name="x">Puun sijinti x-akselilla</param>
    public void LisaaPuu(int x)
    {

        PhysicsObject puu = new PhysicsObject(250, 600);
        puu.X = x;
        puu.Y = Level.Bottom;
        puu.IgnoresGravity = true;
        puu.CanRotate = false;
        puu.Image = puunKuva;
        puu.IgnoresCollisionResponse = true;
        puu.Tag = "puu";
        AddCollisionHandler(pelaaja, "puu", TormaaKuolettavaan);
        Add(puu);

    }



    /// <summary>
    /// Määritellään kerättävä tähti
    /// </summary>
    /// <param name="x">Tähden sijainti x-akselilla</param>
    /// <param name="y">Tähden sijainti y-akselilla</param>
    public void LuoTahti()
    {
        for (int i = 0; i <= 20; i++)
        {
            Tahti tahti = new Tahti(TAHDEN_LEVEYS, TAHDEN_KORKEUS);
            tahti.Image = tahdenKuva;
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
    /// Aliohjelma määrittelee pelikentän ja asettaa sille reunat
    /// </summary>
    public void LuoKentta()
    {
        double[] korkeus = { 3, 3 };

        Level.Width = 20000;
        Level.Height = 1080;
        Level.CreateBorders();

        Gravity = new Vector(0, -100);

        PhysicsObject maanpinta = Level.CreateGround(korkeus, 30);
        maanpinta.Color = Color.Green;
        maanpinta.Tag = "maa";
        //Level.Background.MovesWithCamera = true;
        Level.Background.Image = taustaKuva;
        Level.Background.TileToLevel();
        
        LisaaPelaaja(pelaajanLeveys, pelaajanKorkeus);


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
    /// Aliohjelma määrittelee peliin myrskypilven ja sen ominaisuudet
    /// </summary>
    /// <param name="x">Random-luku, joka määrittää pilven sijainnin x-akselilla</param>
    /// <param name="y">Random-luku, joka määrittää pilven sijainnin y-akselilla</param>
    public void Myrskypilvi(int x, int y)
    {
        PhysicsObject pilvi = new PhysicsObject(200, 250);
        pilvi.X = x;
        pilvi.Y = y;
        pilvi.IgnoresGravity = true;
        pilvi.CanRotate = false;
        pilvi.Image = myrskypilvi;
        pilvi.Tag = "pilvi";
        pilvi.IgnoresCollisionResponse = true;
        AddCollisionHandler(pelaaja, "pilvi", TormaaPilveen);
        Add(pilvi);

    }


    public void SiirraPelaajaaOikealle()
    {
        pelaaja.Push(new Vector(kameranNopeus, 0.0));


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
        

    }


    public void TormaaPilveen(PhysicsObject pelaaja, PhysicsObject kohde)
    {

        Gravity = new Vector(0, -800);
        StopAll();
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Down);
        MessageDisplay.Add("Kuolit, peli loppui!");
        ukkosenAani.Play();
        pelaaja.Image = pelaajaKuoliKuva;

    }



    public void TormaaTahteen(PhysicsObject pelaaja, Tahti tahti)
    {
        tahti.TuhoaTahti();
        pelaajanPisteet.AddValue(1);
        MessageDisplay.Add("Keräsit tähden");
        keraaTahtiAani.Play();
        
    }

   

}

