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
    List<Vector> pilvienSijainnit = new List<Vector>();

    Vector pelaajanPaikkaAlussa = new Vector(-2400, 50);
    private int kameranNopeus = 200;
    const int PELAAJAN_KORKEUS = 70;
    const int KENTAN_LEVEYS = 5000;
    const int KENTAN_KORKEUS = 1080;
    const int TAHDEN_LEVEYS = 50;
    const int TAHDEN_KORKEUS = 50;
    const int LIIKUTUS_VEKTORI = 250;
    const int PILVEN_LEVEYS = 200;
    const int PILVEN_KORKEUS = 250;
    const int VAROETAISYYS = 600;

    // Kuvat ainakin attribuutteina ehdottomasti
    private readonly Image pilvenKuva = LoadImage("myrskypilvi400korkea");
    private readonly Image puunKuva = LoadImage("puu400korkea");
    private readonly Image tahdenKuva = LoadImage("hymyTahti");
    private readonly Image pelaajaKuoli = LoadImage("leija240korkeaKuoli");
    private readonly SoundEffect ukkosenAani = LoadSoundEffect("ukkosenJyrina.wav");
    private readonly SoundEffect keraaTahtiAani = LoadSoundEffect("keraaTahtiAani.wav");
    private readonly SoundEffect oksanKatkeaminen = LoadSoundEffect("oksanKatkeaminen.wav");
    private readonly SoundEffect maahanPutoaminen = LoadSoundEffect("maahanPutoaminen.wav");

    // Nämä attribuutteina mielestäni
    private IntMeter pelaajanPisteet;
    private bool peliKaynnissa = false;
    Timer kameranLiikutusAjastin;

    /// <summary>
    /// Aloittaa pelin
    /// </summary>
    public override void Begin()
    {
        MediaPlayer.Play("taustamusiikkiVaimea.wav");
        MediaPlayer.IsRepeating = true;
        SetWindowSize(1920, KENTAN_KORKEUS); 
        LuoKentta();
        Camera.X = Level.Left + 100;
        peliKaynnissa = true;
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
    /// Pysäyttää pelin pelaajan osuttua johonkin pelin lopettavaan objektiin
    /// Näyttää High Scoret pelin loputtua
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void PeliLoppui(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        peliKaynnissa = false;
        Gravity = new Vector(0, 0);
        kameranLiikutusAjastin.Stop();
        Camera.Velocity = new Vector(0, 0);
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Down);
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);

        double kokonaispisteet = KokonaisPisteet(pelaaja.Position);
        MessageDisplay.Add("Kuljettu matka " + KuljettuMatka(pelaaja.Position));
        MessageDisplay.Add("Keräsit tähtiä " + pelaajanPisteet + " ja  sait yhteensä " + kokonaispisteet + " pistettä.");
        
        if((string)kohde.Tag != "maali")
        {
            pelaaja.Image = pelaajaKuoli;
            Gravity = new Vector(0, -500);
        }

        // High Scoret peliin
        ScoreList parhaatLista = new ScoreList(10, false, 0);
        parhaatLista = DataStorage.TryLoad<ScoreList>(parhaatLista, "leijaParhaatPisteet.xml");
        HighScoreWindow parhaatIkkuna = new HighScoreWindow("Parhaat pisteet", "Onneksi olkoon! Pääsit listalle %p pisteellä!", parhaatLista, kokonaispisteet);
        Add(parhaatIkkuna);
        parhaatIkkuna.Closed += delegate (Window ikkuna)
        {
            DataStorage.Save<ScoreList>(parhaatLista, "leijaParhaatPisteet.xml");
            AloitaAlusta(pelaaja);
        };     
    }


    /// <summary>
    /// Aloittaa pelin alusta
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, jolla pelataan</param>
    public void AloitaAlusta(PhysicsObject pelaaja)
    {
        pelaaja.Destroy();
        Camera.X = Level.Left + 100;
        peliKaynnissa = true;
        LuoKentta();
        LisaaNappaimet(pelaaja);
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
    /// Määrittelee pelikentän ja lisää sinne kaiken pelissä tarvittavan
    /// </summary>
    public void LuoKentta()
    {
        double[] korkeus = { 3, 3 };
        Level.Width = KENTAN_LEVEYS;
        Level.Height = KENTAN_KORKEUS;
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
        
        PhysicsObject pelaaja = LuoPelaaja(PELAAJAN_KORKEUS, PELAAJAN_KORKEUS * 2);
        LuoPuu(pelaaja.Position);
        //LuoPilvi(pelaajanPaikkaAlussa);
        LuoKerattava();
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
    /// Aliohjelma, joka määrittelee pelaajan pelihahmon, sen ominaisuudet ja lisää sen haluttuun paikkaan
    /// </summary>
    /// <param name="leveys">Pelihahmon leveys</param>
    /// <param name="korkeus">Pelihahmon korkeus</param>
    public PhysicsObject LuoPelaaja(double leveys, double korkeus)
    {
        PhysicsObject pelaaja = new PhysicsObject(leveys, korkeus);
        pelaaja.Mass = 0.5;
        pelaaja.Position = pelaajanPaikkaAlussa;
        pelaaja.Image = LoadImage("leija240korkea");
        pelaaja.CanRotate = false;
        Add(pelaaja, 1);
        return pelaaja;
    }


    /// <summary>
    /// Aliohjelma määrittelee peliin myrskypilven ja sen ominaisuudet
    /// </summary>
    /// <param name="pelaajanSijainti">Pelihahmon sijainti pelikentällä pelin alussa</param>
    /// <param name="vihujenSijainnit">Lista, johon tallennetaan kaikkien vihollisten sijainnit pelikentällä</param>
    public void LuoPilvi(Vector pelaajanSijainti)
    {
        for (int i = 0; i < 12; i++)
        {
            int y = RandomGen.NextInt(0, KENTAN_KORKEUS / 2 -50);
            PhysicsObject pilvi = new PhysicsObject(PILVEN_LEVEYS, PILVEN_KORKEUS);
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
            }
            while (Vector.Distance(pilvenSijainti, pelaajanSijainti) < VAROETAISYYS);
            pilvi.Position = pilvenSijainti;

            for (int j = 0; j < pilvienSijainnit.Count; j++)
            {
                while (Vector.Distance(pilvenSijainti, pilvienSijainnit[j]) < VAROETAISYYS)
                {
                    pilvenSijainti = new Vector(RandomGen.NextDouble(Level.Left, Level.Right), y);
                }
            }
            Add(pilvi);
            pilvienSijainnit.Add(pilvenSijainti);
        }
    }


    /// <summary>
    /// Luo peliin puita
    /// </summary>
    /// <param name="pelaajanSijainti">Pelihahmon sijainti pelin alussa</param>
    /// <param name="vihujenSijainnit">Lista vihollisten sijainneista</param>
    public void LuoPuu(Vector pelaajanSijainti)
    {
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

            // Arvotaan puun x niin, ettei se tule liian lähelle pelaajaan alkupistettä
            Vector puunSijainti;
            do
            {
                puunSijainti = new Vector(RandomGen.NextDouble(Level.Left, Level.Right), puu.Y);
            } while (Vector.Distance(puunSijainti, pelaajanSijainti) < VAROETAISYYS);
            puu.Position = puunSijainti;
            Add(puu);
        }
    }


    /// <summary>
    /// Luodaan kentälle kerättävät tähdet
    /// </summary>
    public void LuoKerattava()
    {
        for (int i = 0; i <= 130; i++)
        {
            int x = RandomGen.NextInt(-KENTAN_LEVEYS / 2, KENTAN_LEVEYS / 2);
            int y = RandomGen.NextInt(60, 500);
            PhysicsObject tahti = new PhysicsObject(TAHDEN_LEVEYS, TAHDEN_KORKEUS);
            tahti.Image = tahdenKuva;
            tahti.Tag = "tahti";
            tahti.Position = new Vector(x, y);
            tahti.IgnoresCollisionResponse = true;
            tahti.IgnoresPhysicsLogics = true;
            tahti.IgnoresGravity = true;
            tahti.CanRotate = false;
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
    /// Laskee pelaajan kokonaispisteet kuljetun matkan ja kerättyjen tähtien perusteella
    /// Jokaisesta kuljetusta 1000px matkasta saa 5 lisäpistettä
    /// </summary>
    /// <param name="pelaajanSijaintiKuollessa">Pelihahmon sijainti pelikentällä pelaajan kuollessa</param>
    /// <returns>Pelaajan kokonaispisteet</returns>
    public double KokonaisPisteet(Vector pelaajanSijaintiKuollessa)
    {
        double kuljettuMatka = KuljettuMatka(pelaajanSijaintiKuollessa);
        int i = 0;
        int piste = 5;
        int kerroin = 0;

        while (i < kuljettuMatka)
        {
            if (i % 1000 == 0 && i !=0)
            {
                kerroin++;
            }
            i++;
        }
        double kokonaisPisteet = pelaajanPisteet + kerroin * piste;
        return kokonaisPisteet;
    }


    /// <summary>
    /// Laskee pelihahmon pelin aikan kulkeman kokonaismatkan
    /// </summary>
    /// <param name="pelaajanPaikkaKuollessa">Sijainti, jossa pelaaja oli kuollessaan</param>
    /// <returns>Kuljettu matka</returns>
    public double KuljettuMatka(Vector pelaajanPaikkaKuollessa)
    {
        double kuljettuMatka = Vector.Distance(pelaajanPaikkaAlussa, pelaajanPaikkaKuollessa);
        kuljettuMatka = Convert.ToInt32(kuljettuMatka);
        return kuljettuMatka;
    }


    /// <summary>
    /// Aliohjelma kertoo, mitä tapahtuu, kun pelaaja törmää kuolettavaan esteeseen
    /// kuten pilveen, puuhun tai maahan
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, joka törmää</param>
    /// <param name="kohde">Kohde/vihollinen, johon törmättiin</param>
    public void TormaaPelinLopettavaan(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        if (peliKaynnissa)
        {
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


    /// <summary>
    /// Tapahtuu, kun pelaaja törmää pelissä kerättävään esineeseen esim. tähteen
    /// </summary>
    /// <param name="pelaaja">Pelihahmo, joka törmää</param>
    /// <param name="kerattava">Kerättävä, johon törmätään</param>
    public void TormaaKerattavaan(PhysicsObject pelaaja, PhysicsObject kerattava)
    {
        if (peliKaynnissa)
        {
            kerattava.Destroy();
            pelaajanPisteet.AddValue(1);
            keraaTahtiAani.Play();
        }
    }
}

