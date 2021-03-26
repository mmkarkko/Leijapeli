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
///@version 26.3.2021
/// <summary>
/// Peli, jossa lännetään leijalla, kerätään kerättäviä ja varotaan osumasta mihinkään muuhun.
/// Peli päättyy, jos törmätään johonkin kuolettavaan tai jos pelaaja pääsee maaliin asti.
/// </summary>
public class Leija : PhysicsGame
{
    private readonly PhysicsObject[] pilvienSijainnit = new PhysicsObject[12];
    private readonly PhysicsObject[] kerattavat = new PhysicsObject[130];  
    
    private int kameranNopeus = 200;
    const int KENTAN_PITUUS = 10800;
    const int HAHMON_KOKO = 200;
    
    private IntMeter pelaajanPisteet;
    private bool peliKaynnissa = false;
    private bool ekaPeliKerta = true;
    private Timer kameranLiikutusAjastin;

    /// <summary>
    /// Aloittaa pelin, lisää peliin taustamusiikin
    /// </summary>
    public override void Begin()
    { 
        MediaPlayer.Play("taustamusiikkiVaimea.wav");
        MediaPlayer.IsRepeating = true;
        SetWindowSize(1920, KENTAN_PITUUS/10); 
        LuoKentta();
        Camera.X = Level.Left + 100;
        peliKaynnissa = true;
    }


    /// <summary>
    /// Aloittaa pelin uudestaan pelin päättymisen jälkeen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void AloitaUudelleenAlusta(PhysicsObject pelaaja)
    {
        kameranNopeus = 200;
        pelaaja.Destroy();
        Camera.X = Level.Left + 100;
        peliKaynnissa = true;
        LuoKentta();
        LisaaNappaimet(pelaaja);
    }


    /// <summary>
    /// Liikuttaa kameraa oikealle hiljalleen kasvavalla nopeudella
    /// </summary>
    public void KasvataKameranNopeutta()
    {
        if (peliKaynnissa)
        {
            kameranNopeus += 6;
            Camera.Velocity = new Vector(kameranNopeus, 0);
        }
    }


    /// <summary>
    /// Laskee pelaajan kokonaispisteet kuljetun matkan ja kerättyjen tähtien perusteella
    /// Jokaisesta kuljetusta 1000px matkasta saa 5 lisäpistettä
    /// Maaliin pääsystä saa 100 pistettä lisää
    /// </summary>
    /// <param name="pelaajanSijaintiPelinLopussa">Pelihahmon sijainti pelikentällä pelaajan kuollessa</param>
    /// <param name="kohde">Kohde maali, mikäli peli on päättynyt maaliin</param>
    /// <returns>Pelaajan kokonaispisteet pelin päättyessä</returns>
    public double KokonaisPisteet(Vector pelaajanSijaintiPelinLopussa, PhysicsObject kohde)
    {
        double kuljettuMatka = KuljettuMatka(pelaajanSijaintiPelinLopussa);
        double kokonaisPisteet = 0;
        int i = 0;
        int piste = 5;
        int kerroin = 0;
        double matkaPisteet;

        if ((string)kohde.Tag == "maali") kokonaisPisteet += 100;
        while (i < kuljettuMatka)
        {
            if (i % 1000 == 0 && i != 0)
            {
                kerroin++;
            }
            i++;
        }
        matkaPisteet = kerroin * piste;
        kokonaisPisteet += pelaajanPisteet + matkaPisteet;
        return kokonaisPisteet;
    }


    /// <summary>
    /// Laskee pelihahmon pelin aikan kulkeman kokonaismatkan
    /// </summary>
    /// <param name="pelaajanSijaintiPelinLopussa">Sijainti, jossa pelaaja oli pelin päättyessä</param>
    /// <returns>Pelin aikana kuljettu matka</returns>
    public double KuljettuMatka(Vector pelaajanSijaintiPelinLopussa)
    {
        Vector pelaajanPaikkaAlussa = new Vector(-KENTAN_PITUUS / 2, KENTAN_PITUUS / 50);
        double kuljettuMatka = Vector.Distance(pelaajanPaikkaAlussa, pelaajanSijaintiPelinLopussa);
        kuljettuMatka = Convert.ToInt32(kuljettuMatka);
        return kuljettuMatka;
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
        double liikutusVektori = 250;
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa vasemmalle", pelaaja, new Vector(-liikutusVektori, 0));
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa oikealle", pelaaja, new Vector(liikutusVektori, 0));
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa ylös", pelaaja, new Vector(0, liikutusVektori));
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaPelaajaaNappaimilla, "Liikuta pelaajaa alas", pelaaja, new Vector(0, -liikutusVektori));
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Luo pelikentän ja lisää sinne viholliset, kerattavat, pelaajan
    /// Määrittelee törmäyskäsittelijät sekä kameran liikutuksen
    /// </summary>
    public void LuoKentta()
    {
        double[] korkeus = { 3, 3 };
        Level.Width = KENTAN_PITUUS;
        Level.Height = KENTAN_PITUUS/10;
        Level.CreateLeftBorder();
        Gravity = new Vector(0, -50);
        PhysicsObject oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 0;
        oikeaReuna.Tag = "maali";
        PhysicsObject ylareuna = Level.CreateTopBorder();
        ylareuna.Restitution = 0;
        PhysicsObject maanpinta = Level.CreateGround(korkeus, 30);
        maanpinta.Color = Color.Green;
        maanpinta.Tag = "maa";
        Level.Background.Image = LoadImage("leijaTaustaKuva");
        Level.Background.TileToLevel();
        
        PhysicsObject pelaaja = LuoPelaaja();
        
        LuoKerattava();
        if (ekaPeliKerta) LuoPuu();
        if (ekaPeliKerta) LuoPilvi();
        LisaaNappaimet(pelaaja);

        kameranLiikutusAjastin = new Timer();
        kameranLiikutusAjastin.Interval = 0.5;
        kameranLiikutusAjastin.Timeout += KasvataKameranNopeutta;
        kameranLiikutusAjastin.Start();
        
        pelaajanPisteet = LuoKerattavienLaskuri(Screen.Right - 100, Screen.Top - 100);

        AddCollisionHandler(pelaaja, "tahti", TormaaKerattavaan);
        AddCollisionHandler(pelaaja, "pilvi", TormaaPelinLopettavaan);
        AddCollisionHandler(pelaaja, "puu", TormaaPelinLopettavaan);
        AddCollisionHandler(pelaaja, "maa", TormaaPelinLopettavaan);
        AddCollisionHandler(pelaaja, "maali", TormaaPelinLopettavaan);
    }


    /// <summary>
    /// Luo kentälle kerättävät tähdet
    /// </summary>
    public void LuoKerattava()
    {   
        Image tahdenKuva = LoadImage("hymyTahti");
        for (int i = 0; i < kerattavat.Length; i++)
        {
            int x = RandomGen.NextInt(-KENTAN_PITUUS / 2, KENTAN_PITUUS / 2);
            int y = RandomGen.NextInt(60, 500);
            PhysicsObject tahti = new PhysicsObject(HAHMON_KOKO / 4, HAHMON_KOKO /4);
            tahti.Image = tahdenKuva;
            tahti.Tag = "tahti";
            tahti.Position = new Vector(x, y);
            tahti.IgnoresCollisionResponse = true;
            tahti.IgnoresPhysicsLogics = true;
            tahti.IgnoresGravity = true;
            tahti.CanRotate = false;
            kerattavat[i] = tahti;
            Add(tahti);
        }
    }


    /// <summary>
    /// Luo peliin pistelaskurin näytön
    /// </summary>
    /// <param name="x">Pistelaskurin sijainti x-akselilla</param>
    /// <param name="y">Pistelaskurin sijainti y-akselilla</param>
    /// <returns>Pistelaskuri</returns>
    public IntMeter LuoKerattavienLaskuri(double x, double y)
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
    /// Aliohjelma, joka määrittelee pelaajan pelihahmon, sen ominaisuudet ja lisää sen määrättyyn paikkaan
    /// </summary>
    public PhysicsObject LuoPelaaja()
    {
        PhysicsObject pelaaja = new PhysicsObject(70, 140);
        pelaaja.Mass = 0.5;
        pelaaja.X = -KENTAN_PITUUS / 2;
        pelaaja.Y = KENTAN_PITUUS / 50;
        pelaaja.Image = LoadImage("leijanKuva");
        pelaaja.CanRotate = false;
        Add(pelaaja, 1);
        return pelaaja;
    }


    /// <summary>
    /// Aliohjelma määrittelee peliin myrskypilvi-vihollisen ja sen ominaisuudet
    /// </summary>
    public void LuoPilvi()
    {
        Image pilvenKuva = LoadImage("myrskypilvi");
        double pilvenX;
        double luku = KENTAN_PITUUS / pilvienSijainnit.Length;
        for (int i = 0; i < pilvienSijainnit.Length; i++)
        {
            PhysicsObject pilvi = new PhysicsObject(HAHMON_KOKO, HAHMON_KOKO + 50);
            if (i < 1)  pilvenX = Level.Left + luku;
            else pilvenX = pilvienSijainnit[i - 1].X + luku;
            pilvi.Y = RandomGen.NextDouble(0, Level.Top - 50);
            pilvi.X = pilvenX;
            pilvi.Image = pilvenKuva;
            pilvi.IgnoresCollisionResponse = true;
            pilvi.IgnoresGravity = true;
            pilvi.CanRotate = false;
            pilvi.Tag = "pilvi";
            pilvienSijainnit[i] = pilvi;
            Add(pilvi);
        }
   }


    /// <summary>
    /// Luo peliin puu-viholliset
    /// </summary>
    public void LuoPuu()
    {   
        Image puunKuva = LoadImage("puunKuva");
        double varoetaisyys = 600;
        for (int i = 0; i < 75; i++)
        {
            double puunLeveys = RandomGen.NextInt(100, 300);
            PhysicsObject puu = new PhysicsObject(puunLeveys, 1.8 * puunLeveys);
            puu.Y = Level.Bottom;
            puu.Bottom = Level.Bottom;
            puu.IgnoresGravity = true;
            puu.CanRotate = false;
            puu.Image = puunKuva;
            puu.IgnoresCollisionResponse = true;
            puu.Tag = "puu";

            Vector puunSijainti;
            do
            {
                puunSijainti = new Vector(RandomGen.NextDouble(Level.Left, Level.Right), puu.Y);
            } while (puunSijainti.X !< Level.Left + varoetaisyys);
            puu.Position = puunSijainti;
            Add(puu);
        }
    }


    /// <summary>
    /// Pysäyttää pelin pelaajan osuttua johonkin pelin lopettavaan objektiin
    /// Näyttää High Scoret pelin loputtua
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    /// <param name="kohde">Kohde, johon pelaaja on törmännyt</param>
    public void PeliLoppui(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        Image pelaajaKuoli = LoadImage("leijaKuoli");

        foreach (PhysicsObject kerattava in kerattavat)
        {
            kerattava.Destroy();
        }

        peliKaynnissa = false;
        Gravity = new Vector(0, 0);
        kameranLiikutusAjastin.Stop();
        Camera.Velocity = new Vector(0, 0);
        kameranNopeus = 0;
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Down);
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);

        double kokonaispisteet = KokonaisPisteet(pelaaja.Position, kohde);
        MessageDisplay.Add("Kuljettu matka " + KuljettuMatka(pelaaja.Position));
        MessageDisplay.Add("Keräsit tähtiä " + pelaajanPisteet + " ja  sait yhteensä " + kokonaispisteet + " pistettä.");

        if ((string)kohde.Tag != "maali")
        {
            pelaaja.Image = pelaajaKuoli;
            Gravity = new Vector(0, -500);
        }

        ScoreList parhaatLista = new ScoreList(10, false, 0);
        parhaatLista = DataStorage.TryLoad<ScoreList>(parhaatLista, "leijaParhaatPisteet.xml");
        HighScoreWindow parhaatIkkuna = new HighScoreWindow("Parhaat pisteet", "Onneksi olkoon! Pääsit listalle %p pisteellä!", parhaatLista, kokonaispisteet);
        Add(parhaatIkkuna);
        parhaatIkkuna.Closed += delegate (Window ikkuna)
        {
            DataStorage.Save<ScoreList>(parhaatLista, "leijaParhaatPisteet.xml");
            AloitaUudelleenAlusta(pelaaja);
        };
    }


    /// <summary>
    /// Tapahtuu, kun pelaaja törmää pelissä kerättävään esineeseen esim. tähteen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, joka törmää</param>
    /// <param name="kerattava">Kerättävä, johon törmätään</param>
    public void TormaaKerattavaan(PhysicsObject pelaaja, PhysicsObject kerattava)
    {
        SoundEffect keraaTahtiAani = LoadSoundEffect("kerattavaKerattiinAani.wav");
        if (peliKaynnissa)
        {
            kerattava.Destroy();
            pelaajanPisteet.AddValue(1);
            keraaTahtiAani.Play();
        }
    }


    /// <summary>
    /// Aliohjelma kertoo, mitä tapahtuu, kun pelaaja törmää johonkin kuolettavaan
    /// kuten pilveen, puuhun tai maahan
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, joka törmää</param>
    /// <param name="kohde">Kohde/vihollinen, johon törmättiin</param>
    public void TormaaPelinLopettavaan(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        SoundEffect ukkosenAani = LoadSoundEffect("ukkosenJyrina.wav");
        SoundEffect oksanKatkeaminen = LoadSoundEffect("oksanKatkeaminen.wav");
        SoundEffect maahanPutoaminen = LoadSoundEffect("maahanPutoaminen.wav");

        if (peliKaynnissa)
        {
            ekaPeliKerta = false;
            if ((string)kohde.Tag == "maali") PeliLoppui(pelaaja, kohde);
            else
            {
                PeliLoppui(pelaaja, kohde);
                if ((string)kohde.Tag == "puu") oksanKatkeaminen.Play();
                if ((string)kohde.Tag == "pilvi") ukkosenAani.Play();
                if ((string)kohde.Tag == "maa") maahanPutoaminen.Play();
            }
        }
    }
}

