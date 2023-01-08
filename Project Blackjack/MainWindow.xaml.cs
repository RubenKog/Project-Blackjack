using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;
using System.Reflection;

namespace Project_Blackjack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Globale declaraties:

        //Nodig voor kaartgeneratie:
        //KaartCode is essentieel voor identificatie in afbeeldingen en arrays.
        Random rnd = new Random();
        int RndType;
        int RndWaarde;
        int KaartCode;
        string VerborgenType;
        string VerborgenWaarde;
        int AantalKaartSpeler = 0;
        int AantalKaartBank = 0;
        int AantalKaartTotaal = 0;
        string KaartType;
        string KaartWaarde;
        //Arrays met getrokken kaarten: 
        int[] AlGetrokkenSpeler = new int[11];
        int[] AlGetrokkenBank = new int[11];
        int[] AlGetrokkenGame = new int[52];
        //Kapitaal (Inzet en budget)
        bool CustomKapitaal = false;
        float SpelerInzet;
        float SpelerBudget = 0;
        //Score bijhouden
        int KaartScore;
        int ScoreSpeler = 0;
        int ScoreBank = 0;
        //Aantal azen
        int AasSpeler = 0;
        int AasBank = 0;
        //Speler of bank een kaart geven?
        bool IsSpeler = true;
        //Kaart geselecteerd
        bool UitLijstBank = false;
        bool UitLijstSpeler = false;
        bool UitAutoLijst = false;
        int LijstIndex;
        bool BankVerborgenKaart = false;
        bool AutoCardRotated = false;
        bool DraaiKaart = false;
        bool Drankje = false;
        //Muziek
        SoundPlayer musicPlayer = new SoundPlayer(Properties.Resources.GroovyTower);
        bool MusicPlaying = false;
        //Klok
        private DispatcherTimer klok = new DispatcherTimer();
        //Gamronde afgelopen?
        bool rondeVoltooid = false;
        //Historiek
        StringBuilder HistoriekSB = new StringBuilder();
        List <string> HistoriekList = new List<string>();
        int RondeCounter = 0;
        string HistoriekTekst;
        float RondeBudget = 0;
        public MainWindow()
        {
            InitializeComponent();
            klok.Tick += new EventHandler(Klok_Ticked);
            klok.Interval = new TimeSpan(0, 0, 1);
            klok.Start();
        }

        /// <summary>
        /// Elke seconde: Update de klok
        /// </summary>
        private void Klok_Ticked(object sender, EventArgs e)
        {
            TxtKlok.Content = $"Tijd: {DateTime.Now.ToLongTimeString()}";
        }

        /// <summary>
        /// <para>BtnDeel_Click controleert of de speler geld heeft om te spelen, vraagt inzet en regelt wie kaarten krijgt.
        /// async wordt gebruikt om 500 mseconden vertragingen te krijgen met await Task.Delay(500).</para>
        /// <para>Indien de ronde voltooid is wordt de deelknop (en dus ook deze method) gebruikt
        /// om het veld vrij te maken en de ronde te resetten</para>
        /// </summary>
        /// <param name="sender">De sender is altijd de BtnDeel, die ook dient om een nieuwe ronde te starten.</param>       
        private async void BtnDeel_Click(object sender, RoutedEventArgs e)
        { 
            BtnReset.IsEnabled = false;
            //Delen bij start van spel
            if (rondeVoltooid == false)
            {
                //Indien budget = 0, geef nieuw budget
                while (SpelerBudget == 0)
                {
                    TxtGeld.Content = $"Budget = {SpelerBudget}";
                    //Geef een custom kapitaal indien gewenst.
                    if (CustomKapitaal == true)
                    {
                        Start_Kapitaal();
                    }
                    else if (CustomKapitaal == false)
                    {
                        SpelerBudget = 100;
                    }
                    TxtGeld.Content = $"Budget = {SpelerBudget}";
                }
                //Laat speler geld inzetten
                Inzet();
                //Start delen
                DraaiKaart = false;
                BtnDeel.IsEnabled = false;
                RondeCounter++;
                IsSpeler = true;
                Geef_Kaart();
                await Task.Delay(500);
                Geef_Kaart();

                while (ScoreSpeler > 21 && AasSpeler > 0)
                {
                    ScoreSpeler -= 10;
                    AasSpeler--;
                }
                TxtSScore.Content = ScoreSpeler.ToString();
                await Task.Delay(500);
                IsSpeler = false;
                Geef_Kaart();
                //De tweede kaart van de bank is verborgen voor de speler.
                BankVerborgenKaart = true;
                await Task.Delay(500);
                Geef_Kaart();
                BtnHit.IsEnabled = true;
                BtnStand.IsEnabled = true;
                //Men kan enkel de inzet verdubbelen indien daar nog voldoende budget voor is.
                if (SpelerBudget >= (SpelerInzet))
                {
                    BtnDubbel.IsEnabled = true;
                }
            }
            //Deelknop is resetknop bij einde van spelronde
            if (rondeVoltooid == true)
            {
                Gameronde_Reset();                
            }
            //Indien speler of bank wint met de eerste 2 kaarten moet dit onmiddelijk herkend worden
            if (ScoreSpeler == 21)
            {
                BankVerborgenKaart = false;
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                TxtBScore.Content = ScoreBank.ToString();
                LijstBank.SelectedIndex = 1;
                Game_Einde();
            }
            if (ScoreSpeler != 21 && ScoreBank == 21 && BankVerborgenKaart == true)
            {
                BankVerborgenKaart = false;
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                TxtBScore.Content = ScoreBank.ToString();
                Drankje = true;
                Afbeelding_Wijzigen();
                MessageBox.Show($"De bank behaalde een BlackJack tijdens het delen, als troost krijg je een drankje van het huis.", "Verfrissend!");
                Game_Einde();
            }
            BtnReset.IsEnabled = true;
        }
        /// <summary>
        /// <para>BtnHit_Click dient enkel om extra kaarten te nemen als speler.</para>
        /// </summary>
        /// <param name="sender">Sender is steeds BtnHit</param>        
        private async void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            IsSpeler = true;
            await Task.Delay(250);
            Geef_Kaart();

            //Aanpassen waarde van azen indien nodig.
            while (ScoreSpeler > 21 && AasSpeler > 0)
            {
                ScoreSpeler -= 10;
                AasSpeler--;
            }
            TxtSScore.Content = ScoreSpeler.ToString();

            //Indien blackjack, toon de verborgen kaart van de bank en stop de ronde.
            if (ScoreSpeler >= 21)
            {
                await Task.Delay(500);
                BankVerborgenKaart = false;
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                TxtBScore.Content = ScoreBank.ToString();
                //De geselecteerde index wijzigen triggert LijstBank_SelectionChanged() zodat de getoonde kaartafbeelding wijzigt.
                LijstBank.SelectedIndex = 1;
                Game_Einde();
            }
            else
            {
                BtnHit.IsEnabled = true;
                BtnStand.IsEnabled = true;
            }
            BtnReset.IsEnabled = true;
        }
        /// <summary>
        /// <para>BtnStand_Click geeft de bank kaarten tot deze een minimumscore van 17 bereikt.</para>
        /// </summary>
        /// <param name="sender"> De sender is steeds BtnStand of BtnDubbel_Click</param>        
        private async void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnHit.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            IsSpeler = false;
            //GeefAanBank is noodzakelijk: Anders stopt bank met kaarten krijgen na overschrijden van 21 met een aas in bezit
            bool geefAanBank = true;
            while (geefAanBank == true)
            {
                if (BankVerborgenKaart == true)
                {

                    BankVerborgenKaart = false;
                    LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                    TxtBScore.Content = ScoreBank.ToString();
                    LijstBank.SelectedIndex = 1;
                }
                while (ScoreBank < 17)
                {
                    await Task.Delay(500);
                    Geef_Kaart();
                }
                //Aanpassen waarde van azen indien nodig.
                while (ScoreBank > 21 && AasBank > 0)
                {
                    ScoreBank -= 10;
                    AasBank--;
                }
                TxtBScore.Content = ScoreBank.ToString();
                if (ScoreBank >= 17)
                {
                    geefAanBank = false;
                }
            }

            if (ScoreBank >= 17)
            {
                Game_Einde();
            }
            BtnReset.IsEnabled = true;

        }
        /// <summary>
        /// <para>BtnMusic_Used speelt muziek af.</para>
        /// <para>Omdat BtnMusic een checkbox is hangt de respons af van MusicPlaying.</para>
        /// </summary>
        /// <param name="sender">BtnMusic is steeds de sender</param>
        
        private void BtnMusic_Used(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            Drankje = true;
            Afbeelding_Wijzigen();
            if (MusicPlaying == false)
            {
                musicPlayer.PlayLooping();
                MusicPlaying = true;
            }
            else
            {
                musicPlayer.Stop();
                MusicPlaying = false;
            }
            BtnReset.IsEnabled = true;
        }
        /// <summary>
        /// <para>BtnKapitaal_Used beheert of de speler een zelfgekozen startkapitaal wilt bij aanvang van het spel.</para>
        /// <para>Indien het spel al bezig is krijgt de speler een mededeling dat dit pas van toepassing is zodra hun huidige budget op is.</para>
        /// </summary>
        /// <param name="sender">BtnKapitaal is steeds de zender</param>
        /// <param name="e"></param>
        private void BtnKapitaal_Used(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            if (CustomKapitaal == true)
            {
                CustomKapitaal = false;
            }
            else if (CustomKapitaal == false)
            {
                CustomKapitaal = true;
                if (SpelerBudget > 0)
                {
                    MessageBox.Show($"Het spel is al bezig. Deze instelling zal activeren bij het starten van een nieuw spel.", "Spel al bezig");
                }
            }
            BtnReset.IsEnabled = true;
        }
        /// <summary>
        /// <para>Men komt in Start_Kapitaal terecht via BtnDeel_Click indien CustomKapitaal = true</para>
        /// <para>De speler kan een zelfgekozen waarden ingeven. Deze moet groter zijn dan 0 en in een float passen.</para>
        /// <para>Indien de speler een kommagetal ingeeft zal dit naar beneden afgerond worden (De speler krijgt geen bonus van het casino)</para>
        /// <para>De minimuminzet is 1, dus een budget kleiner dan dit wordt onmiddelijk uitgegeven aan drankjes.</para>
        /// </summary>
        private void Start_Kapitaal()
        {
            bool BudgetOK = false;

            while (BudgetOK == false)
            {
                BudgetOK = float.TryParse(Interaction.InputBox("Geef startkapitaal (afronding naar beneden)", "Invoer", ""), out SpelerBudget);
                if (BudgetOK == false)
                {
                    MessageBox.Show("Dat is een foute of te grote invoer, probeer opnieuw door een getal in te voeren.", "Foutieve invoer");
                }
                if (BudgetOK == true && SpelerBudget <= 0)
                {
                    MessageBox.Show("Je budget moet groter zijn dan 0.", "Foutieve invoer");
                    BudgetOK = false;
                }
            }
            if (SpelerBudget >= 1)
            {
                SpelerBudget = Convert.ToSingle(Math.Floor(SpelerBudget));
            }
            if (SpelerBudget < 1)
            {
                MessageBox.Show($"Je hebt een lekker drankje gekocht met je ${SpelerBudget} want dit was te laag om in te zetten.", "Verfrissend!");
                SpelerBudget = 0;
                Drankje = true;
                Afbeelding_Wijzigen();
            }
        }
        /// <summary>
        /// <para>Gelijkaardig aan Start_Kapitaal, Inzet() laat de speler geld inzetten. De minimuminzet (afgerond naar boven) is 10% van het budget. (De laagst mogelijke inzet = 1)</para>
        /// </summary>
        private void Inzet()
        {
            bool InzetOK = false;
            while (InzetOK == false)
            {
                InzetOK = float.TryParse(Interaction.InputBox("Geef inzet (afronding naar boven).", "Geef inzet", ""), out SpelerInzet);
                if (InzetOK == false)
                {
                    MessageBox.Show("Dat is een foute invoer, geef een getal.", "Foutieve invoer");
                }
                if (InzetOK == true && SpelerInzet <= 0)
                {
                    MessageBox.Show("De inzet moet groter zijn dan 0.", "Foutieve invoer");
                    InzetOK = false;
                }
                if (InzetOK == true && SpelerBudget < SpelerInzet)
                {
                    MessageBox.Show($"Je budget is maar {SpelerBudget}, gelieve je inzet te verlagen.", "Foutieve invoer");
                    InzetOK = false;
                }
                if (InzetOK == true && SpelerInzet < (Convert.ToSingle(Math.Ceiling(SpelerBudget / 10))))
                {
                    MessageBox.Show($"Je inzet is maar ${SpelerInzet}, je moet minstens ${Math.Ceiling(SpelerBudget / 10)} inzetten om te spelen.", "Foutieve invoer");
                    InzetOK = false;
                }
                if(InzetOK == true && Convert.ToSingle(Math.Ceiling(SpelerInzet)) > SpelerBudget )
                {
                    MessageBox.Show($"Je inzet is (afgerond naar boven) ${Convert.ToSingle(Math.Ceiling(SpelerInzet))}, maar je hebt maar ${SpelerBudget}.", "Foutieve invoer");
                    InzetOK = false;
                }
            }

            SpelerInzet = Convert.ToSingle(Math.Ceiling(SpelerInzet));
            TxtGeld.Content = $"Budget = {SpelerBudget} (-{SpelerInzet})";
            RondeBudget = SpelerBudget;
            SpelerBudget -= SpelerInzet;
        }
        /// <summary>
        /// <para>Geef_Kaart() beheert getrokken kaarten na controle en houdt bij hoeveel er getrokken zijn. IsSpeler bepaalt wie de kaart mag krijgen.</para>
        /// <para>De arrays die getrokken kaarten bijhouden worden hier aangepast, net zoals de listboxen zodat de speler een overzicht heeft van alle getrokken kaarten.</para>
        /// <para>Behalve bij verborgen kaarten zal de score een update krijgen, en de afbeeldingen aangepast worden via KaartStapel_Update() en Afbeelding_Wijzigen().</para>
        /// </summary>
        private void Geef_Kaart()
        {
            //Controle of de kaart gegeven moet worden aan speler of bank
            //Updaten van de kaartlijsten en score
            //Bijhouden van aantal getrokken azen
            //Controle of kaart al getrokken is
            //afbeelding wijzigen
            if (IsSpeler == true)
            {
                Kaart_Controleren();
                if (RndWaarde == 1)
                {
                    AasSpeler++;
                }
                AlGetrokkenSpeler[AantalKaartSpeler] = KaartCode;
                AantalKaartSpeler++;
                AlGetrokkenGame[AantalKaartTotaal] = KaartCode;
                AantalKaartTotaal++;
                KaartStapel_Update();
                ScoreSpeler += KaartScore;
                LijstSpeler.Items.Add($"{KaartType} {KaartWaarde}");
                TxtAantalKaarten.Content = $"Aantal kaarten over: {52 - AantalKaartTotaal}";
                TxtSScore.Content = ScoreSpeler.ToString();
            }
            else if (IsSpeler == false)
            {
                Kaart_Controleren();
                if (RndWaarde == 1)
                {
                    AasBank++;
                }
                AlGetrokkenBank[AantalKaartBank] = KaartCode;
                AantalKaartBank++;
                AlGetrokkenGame[AantalKaartTotaal] = KaartCode;
                AantalKaartTotaal++;
                TxtAantalKaarten.Content = $"Aantal kaarten over: {52 - AantalKaartTotaal}";

                ScoreBank += KaartScore;
                if (BankVerborgenKaart == false)
                {
                    LijstBank.Items.Add($"{KaartType} {KaartWaarde}");
                    TxtBScore.Content = ScoreBank.ToString();
                }
                else
                {
                    LijstBank.Items.Add($"Verborgen");
                }
            }
            UitAutoLijst = true;
            if (BankVerborgenKaart == false || IsSpeler == true || ScoreBank >= 21)
            {
                Afbeelding_Wijzigen();
            }
        }
        /// <summary>
        /// <para>Kaart_Controleren voert een controle uit op getrokken kaarten. Ze mogen niet op het bord liggen en ook niet uit het deck verwijderd zijn.</para>
        /// <para>Het is mogelijk dat er kaarten op het veld liggen die toch nog trekbaar zijn volgens het deck. Dit kan gebeuren tijdens een shuffle.</para>
        /// </summary>
        private void Kaart_Controleren()
        {
            bool kaartAlGetrokken = true;
            while (kaartAlGetrokken == true)
            {
                Kaart_Trekken();
                kaartAlGetrokken = false;
                for (int i = 0; i < 11; i++)
                {
                    //Controle: Kaart al opgelegd?
                    if (KaartCode == AlGetrokkenSpeler[i])
                    {
                        kaartAlGetrokken = true;
                    }
                    if (KaartCode == AlGetrokkenBank[i])
                    {
                        kaartAlGetrokken = true;
                    }
                }
                //Controle: Kaart nog in deck?
                foreach (int waarde in AlGetrokkenGame)
                {
                    if (KaartCode == waarde)
                    {
                        kaartAlGetrokken = true;
                    }
                }
            }
        }
        /// <summary>
        ///  KaartStapel_Update() Past de stapelafbeelding aan bij het bereiken van verschillende milestones.
        /// </summary>
        private void KaartStapel_Update()
        {
            string PileSize = "CardBackgroundFullDeck";
            
            if((AantalKaartTotaal) >= 51)
            {
                PileSize = "CardBackgroundSolo";
            }
            else if ((52 - AantalKaartTotaal) < 3)
            {
                PileSize = "CardBackgroundDuo";
            }
            else if ((52 - AantalKaartTotaal) < 15)
            {
                PileSize = "CardBackgroundSmallDeck";
            }
            else if ((52 -AantalKaartTotaal) < 35)
            {
                PileSize = "CardBackgroundBigDeck";
            }
            else if ((52 - AantalKaartTotaal) < 45)
            {
                PileSize = "CardBackgroundBigDeck";
            }
            BitmapImage KaartStapel = new BitmapImage();
            KaartStapel.BeginInit();
            KaartStapel.UriSource = new Uri($"{PileSize}.png", UriKind.Relative);
            KaartStapel.EndInit();
            ImgKaartStapel.Source = KaartStapel;
        }

        /// <summary>
        /// <para>Kaart_Trekken schudt het deck indien de kaarten op zijn. (Kaarten dan nog op het veld keren terug naar het deck na die ronde)</para>
        /// <para>Twee willekeurige getallen worden opgeroepen: RndType en RndWaarde. Deze worden geïnterpreteerd en naast elkaar geplaatst om de getrokken kaart te vormen</para>
        /// <para>Kaartcode is de unieke identificatie voor een kaart. Deze wordt gebruikt bij Kaart_Controleren() (kaart mag niet gespeeld worden indien uit deck of op veld) en Afbeelding_Wijzigen()</para>
        /// <para>Verborgen kaarten doorlopen bijna hetzelfde traject als normale kaarten, maar worden ook in VerborgenType en VerborgenWaarde bijgehouden</para>
        /// 
        /// <para>Een alternatief voor de vele "if"-statements zou een dictionary zijn. Maar aangezien dit pas laat in het semester werd behandeld en de huidige schrijfmethode duidelijker is bij het zoeken naar fouten werd ervoor gekozen dit niet aan te passen.</para>
        /// </summary>
        private void Kaart_Trekken()
        {

            if (AantalKaartTotaal == 52)
            {

                Drankje = true;
                Afbeelding_Wijzigen();
                TxtAantalKaarten.Content = $"Aantal kaarten over: {52 - AantalKaartTotaal}";
                MessageBox.Show("Alle kaarten zijn gespeeld. Het deck wordt opnieuw geshuffled. Eventuele kaarten al op de tafel blijven daar tot het einde van de ronde. Geniet ondertussen van een drankje.", "Shuffle ");
                Array.Clear(AlGetrokkenGame, 0, AlGetrokkenGame.Length);                
                AantalKaartTotaal = 0;                
                KaartStapel_Update();
            }
            //Generatie van KaartType en -waarde.
            RndType = rnd.Next(1, 5);
            RndWaarde = rnd.Next(1, 14);
            KaartCode = (RndType * 100) + RndWaarde;

            //Interpretatie van type en waarde
            if (RndType == 1)
            {
                KaartType = "Harten";
            }
            else if (RndType == 2)
            {
                KaartType = "Ruiten";
            }
            else if (RndType == 3)
            {
                KaartType = "Schoppen";
            }
            else if (RndType == 4)
            {
                KaartType = "Klaveren";
            }

            if (RndWaarde == 1)
            {
                KaartWaarde = "aas (1 of 11)";
                KaartScore = 11;
            }

            if (RndWaarde < 11 && RndWaarde > 1)
            {
                KaartWaarde = $"{RndWaarde}";
                KaartScore = RndWaarde;
            }
            else if (RndWaarde > 10)
            {

                KaartScore = 10;
                if (RndWaarde == 11)
                {
                    KaartScore = 10;
                    KaartWaarde = "boer (10)";
                }
                else if (RndWaarde == 12)
                {
                    KaartScore = 10;
                    KaartWaarde = "dame (10)";
                }
                else if (RndWaarde == 13)
                {
                    KaartScore = 10;
                    KaartWaarde = "koning (10)";
                }
            }
            if (BankVerborgenKaart == true && IsSpeler == false)
            {
                VerborgenType = KaartType;
                VerborgenWaarde = KaartWaarde;
            }
        }
        /// <summary>
        /// <para>Bij afloop van een ronde wordt gecontroleerd welk einde toepasselijk is.</para>
        /// </summary>
        private void Game_Einde()
        {
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            BtnDeel.IsEnabled = true;
            rondeVoltooid = true;
            BtnDeel.Content = "Nieuwe Ronde";

            if (ScoreSpeler < 21)
            {
                if (ScoreBank > 21)
                {
                    Game_Gewonnen();
                }
                else if (ScoreSpeler > ScoreBank)
                {
                    Game_Gewonnen();
                }
                else if (ScoreSpeler < ScoreBank)
                {
                    Game_Verloren();
                }
                else if (ScoreSpeler == ScoreBank)
                {
                    Game_Push();
                }

            }
            else if (ScoreSpeler > 21)
            {
                Game_Verloren();
            }
            else if (ScoreSpeler == 21)
            {
                TxtBScore.Content = ScoreBank.ToString();
                MessageBox.Show($"Je behaalde een BlackJack!", "BlackJack!");
                Game_Gewonnen();
            }
        }
        /// <summary>
        /// <para>Indien de speler wint krijgt deze zijn inzet dubbel terug, *2,5 indien gewonnen met een blackjack.</para>
        /// <para>De overwinning is zichtbaar met een groene "Gewonnen"</para>
        /// </summary>
        private void Game_Gewonnen()
        {
            TxtStatus.Content = "Gewonnen";
            TxtStatus.Foreground = Brushes.LightGreen;
            if (ScoreSpeler == 21)
            {
                SpelerBudget = SpelerBudget + ((Convert.ToSingle(5.0 / 2.0) * SpelerInzet));
            }

            else if (ScoreSpeler < 21)
            {
                SpelerBudget = SpelerBudget + (2 * SpelerInzet);
            }
            TxtGeld.Content = $"Budget = {SpelerBudget}";
        }

        /// <summary>
        /// <para>Indien de speler verliest krijgt deze zijn inzet niet terug.</para>
        /// <para>Het verlies is zichtbaar met een rode "Verloren"</para>
        /// </summary>
        private void Game_Verloren()
        {
            TxtStatus.Content = "Verloren";
            TxtStatus.Foreground = Brushes.Red;
            TxtGeld.Content = $"Budget = {SpelerBudget}";
        }

        /// <summary>
        /// <para>Indien de speler en bank gelijk eindigen krijgt de speler zijn inzet terug.</para>
        /// <para>De push is zichtbaar met een zwarte "Push"</para>
        /// </summary>
        private void Game_Push()
        {
            LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
            TxtStatus.Content = "Push";
            SpelerBudget += SpelerInzet;
            TxtGeld.Content = $"Budget = {SpelerBudget}";
        }

        /// <summary>
        /// <para>Gameronde_Reset()zet settings terug goed voor de start van een nieuwe ronde via RondeData_Reset.</para>
        /// <para>Deze method is oproepbaar via BtnDeel_Click() indien een ronde voltooid is.</para>
        /// <para>De vorige ronde opgeslagen in de historiek.</para>
        /// <para>Indien de speler geen geld meer heeft, of een budget kleiner dan 1 (haalbaar via blackjack *2,5 winst) krijgt de speler een game-over boodschap.</para>
        /// </summary>
        private void Gameronde_Reset()
        {
            if (rondeVoltooid == true)
            {
                HistoriekTekst = $"{SpelerBudget - RondeBudget} - {ScoreSpeler} / {ScoreBank}";
                TxtHistoriek.Content = $"Historiek: {HistoriekTekst}";
                HistoriekTekst = $"{RondeCounter}: {SpelerBudget - RondeBudget} - {ScoreSpeler} / {ScoreBank}";
                HistoriekList.Add(HistoriekTekst);                
                HistoriekList.Reverse();
                HistoriekSB.Clear();
                for(int i = 0; i < HistoriekList.Count; i++)
                {
                    HistoriekSB.AppendLine($"{HistoriekList[i]}");
                }
                HistoriekList.Reverse();
                RondeData_Reset();
            }
            if (SpelerBudget < 1)
            {
                if (SpelerBudget == 0)
                {
                    Drankje = true;
                    Afbeelding_Wijzigen();
                    MessageBox.Show($"Je hebt helaas geen geld meer, maar je krijgt een drankje van het huis!", "Verfrissend!");
                    TxtGeld.Content = $"Budget = {SpelerBudget}";
                }
                if (SpelerBudget > 0)
                {
                    Drankje = true;
                    Afbeelding_Wijzigen();
                    MessageBox.Show($"Je hebt nog maar ${SpelerBudget} over. Te weinig om verder te spelen. Gelukkig zijn de drankjes goedkoop.", "Verfrissend!");
                    SpelerBudget = 0;
                    TxtGeld.Content = $"Budget = {SpelerBudget}";

                }
            }
        }

        /// <summary>
        /// AllData_Reset geeft het spel een volledige reset, en geeft de speler een drankje. 
        /// </summary>
        private void AllData_Reset()
        {
            TxtGeld.Content = "Budget: ---";
            TxtAantalKaarten.Content = "Aantal Kaarten in spel: 52";
            BtnHit.IsEnabled = false;
            BtnReset.IsEnabled = true;
            BtnStand.IsEnabled = false;
            RondeData_Reset();
            RondeBudget = 0;
            RondeCounter = 0;
            SpelerBudget = 0;
            AantalKaartTotaal = 0;
            Array.Clear(AlGetrokkenGame, 0, AlGetrokkenGame.Length);          
            HistoriekTekst = "";
            TxtHistoriek.Content = "Historiek: .. - .. / ..";
            HistoriekList.Clear();
            HistoriekSB.Clear();
            AasSpeler = 0;
            AasBank = 0;
            UitAutoLijst =false;
            UitLijstSpeler=false;
            UitLijstBank=false;
            BankVerborgenKaart = false;
            AutoCardRotated = false;
            DraaiKaart = false;
            Drankje = true;
            Afbeelding_Wijzigen();
            rondeVoltooid = false;
            BtnDeel.IsEnabled = true;
        }

        /// <summary>
        /// RondeData_Reset zorgt ervoor dat de volgende ronde kan starten zonder onnodige info mee te nemen van de afgelopen ronde.
        /// </summary>
        private void RondeData_Reset()
        {
            AutoCardRotated = false;
            BankVerborgenKaart = false;
            TxtStatus.Foreground = Brushes.Black;
            TxtStatus.Content = "";
            TxtBScore.Content = "0";
            TxtSScore.Content = "0";
            ScoreSpeler = 0;
            ScoreBank = 0;
            LijstBank.Items.Clear();
            LijstSpeler.Items.Clear();
            AasSpeler = 0;
            AasBank = 0;
            SpelerInzet = 0;
            for (int i = 0; i < 11; i++)
            {
                AlGetrokkenBank[i] = 0;
                AlGetrokkenSpeler[i] = 0;

            }
            AantalKaartSpeler = 0;
            AantalKaartBank = 0;
            BtnDeel.Content = "Deal Cards";
            rondeVoltooid = false;
        }
        /// <summary>
        /// <para>Alle kaarten op het veld zijn toonbaar voor spelers door ze aan te klikken in de lijst.</para>
        /// <para>LijstSpeler_SelectionChanged zorgt ervoor dat de kaart-update plaatsvind.</para>
        /// </summary>
        /// <param name="sender">De sender is LijstSpeler (listbox)</param>
        private void LijstSpeler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LijstSpeler.SelectedIndex > -1)
            {
                UitLijstBank = false;
                UitLijstSpeler = true;
                LijstIndex = LijstSpeler.SelectedIndex;
                Afbeelding_Wijzigen();
                LijstSpeler.SelectedIndex = -1;
            }
        }
        /// <summary>
        /// <para>LijstBank_SelectionChangedis hetzelfde als LijstSpeler_SelectionChanged maar voor de bank, met uizondering voor de verborgen kaart. </para>        
        /// </summary>
        /// <param name="sender">De sender is LijstBank (listbox)</param>
        private void LijstBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LijstBank.SelectedIndex > -1)
            {
                if (((LijstBank.SelectedItem)).ToString() != "Verborgen")
                {
                    UitLijstSpeler = false;
                    UitLijstBank = true;
                    LijstIndex = LijstBank.SelectedIndex;
                    Afbeelding_Wijzigen();
                }
                LijstBank.SelectedIndex = -1;
            }
        }
        /// <summary>
        /// <para>Afbeelding_Wijzigen past de afbeelding van de getoonde kaart aan.</para>
        /// <para>De naam van de te trekken kaart wordt in arrayWaarde gestoken. Deze is afkomstig uit de arrays met getrokken kaarten (selectie via lijstindex), of omdat deze waarde nog in KaartCode zit.</para>
        /// <para>UitAutoLijst is voor het automatisch tonen van de laatst getrokken kaart</para>
        /// <para>Met DraaiKaart, AutoCardRotated en rotatieKaart wordt de kaart op gedraaide wijze getoond indien van toepassing (Wegens Double Down)</para>
        /// <para>Indien Drankje = true wordt een willekeurige afbeelding van een drankje getoond.</para>
        /// </summary>
        private void Afbeelding_Wijzigen()
        {

            int arrayWaarde = 0;
            bool rotatieKaart = false;
            if (UitLijstSpeler == true)
            {
                arrayWaarde = AlGetrokkenSpeler[LijstIndex];
                UitLijstSpeler = false;
                if (LijstIndex == 2 && DraaiKaart == true)
                {
                    rotatieKaart = true;
                }
            }
            else if (UitLijstBank == true)
            {
                arrayWaarde = AlGetrokkenBank[LijstIndex];
                UitLijstBank = false;
                rotatieKaart = false;
            }
            else if (UitAutoLijst == true)
            {
                arrayWaarde = KaartCode;
                UitAutoLijst = false;
                if (DraaiKaart == true && AutoCardRotated == false)
                {
                    rotatieKaart = true;
                    AutoCardRotated = true;
                }
            }

            if (rotatieKaart)
            {
                BitmapImage RotatedKaart = new BitmapImage();
                RotatedKaart.BeginInit();
                RotatedKaart.UriSource = new Uri($"{arrayWaarde}.PNG", UriKind.Relative);
                RotatedKaart.Rotation = Rotation.Rotate270;
                RotatedKaart.EndInit();
                ImgKaart.Source = RotatedKaart;

            }
            else
            {
                BitmapImage NormalKaart = new BitmapImage();
                NormalKaart.BeginInit();
                NormalKaart.UriSource = new Uri($"{arrayWaarde}.PNG", UriKind.Relative);
                NormalKaart.EndInit();
                ImgKaart.Source = NormalKaart;
            }

            if (Drankje)
            {
                arrayWaarde = rnd.Next(501, 514);

                BitmapImage DrankjeKaart = new BitmapImage();
                DrankjeKaart.BeginInit();
                DrankjeKaart.UriSource = new Uri($"{arrayWaarde}.PNG", UriKind.Relative);
                DrankjeKaart.EndInit();
                ImgKaart.Source = DrankjeKaart;
                Drankje = false;

            }
        }
        /// <summary>
        /// <para>BtnDubbel is enkel beschikbaar onder omstandigheden waarin de speler een Double Down mag doen.</para>
        /// <para>Btndubbel_Click verdubbelt de inzet van de speler, geeft de speler 1 kaart (geroteerd) en voert ten slotte BtnStand_Click() uit</para>
        /// </summary>
        /// <param name="sender">De sender is BtnDubbel</param>        
        private async void BtnDubbel_Click(object sender, RoutedEventArgs e)
        {
            SpelerBudget -= SpelerInzet;
            SpelerInzet *= 2;
            TxtGeld.Content = $"Budget = {RondeBudget} (-{SpelerInzet})";
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            DraaiKaart = true;
            IsSpeler = true;
            Geef_Kaart();

            while (ScoreSpeler > 21 && AasSpeler > 0)
            {
                ScoreSpeler -= 10;
                AasSpeler--;
            }
            TxtSScore.Content = ScoreSpeler.ToString();
            BtnHit.IsEnabled = true;
            BtnStand.IsEnabled = true;
            await Task.Delay(500);
            if (ScoreSpeler >= 21)
            {
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                LijstBank.SelectedIndex = 1;
                Game_Einde();
            }
            else
            {
                LijstBank.SelectedIndex = 1;
                TxtBScore.Content = ScoreBank.ToString();
                BtnStand_Click(sender, e);
            }
        }

        /// <summary>
        /// <para>Dit waarschuwt de speler om te voorkomen dat deze het spel per ongeluk reset.</para>
        /// <para>Na bevestiging start AllData_Reset()</para>
        /// </summary>
        /// <param name="sender">BtnRestart is de sender</param>       
        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bent u zeker? Uw budget zal resetten, de historiek zal gewist worden en het deck zal opnieuw geschud worden.", "Herstarten", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {               
                AllData_Reset();
            }
        }
        /// <summary>
        /// Indien de speler klikt op TxtHistoriek krijgt de speler een rondeoverzicht te zien.
        /// </summary>
        /// <param name="sender">De sender is TxtHistoriek</param>        
        private void Historiek_Click(object sender, MouseButtonEventArgs e)
        {
            StringBuilder OverzichtSB = new StringBuilder();
            string allHistoriek = HistoriekSB.ToString();
            OverzichtSB.AppendLine("Dit is het overzicht van alle speelronden, ingedeeld volgens deze structuur:");
            OverzichtSB.AppendLine("{rondenummer}: {gewonnen of verloren bedrag} – {score speler} / {score bank}");
            OverzichtSB.AppendLine();
            OverzichtSB.AppendLine(allHistoriek);
            allHistoriek = OverzichtSB.ToString();
            MessageBox.Show(allHistoriek, "Overzicht");
            OverzichtSB.Clear();
        }
        /// <summary>
        /// BtnCredits_Click dient om de speler te tonen wie aan het project heeft gewerkt en waar de afbeeldingen/muziek vandaan komen.
        /// </summary>
        /// <param name="sender">Dit is steeds BtnCredits</param>        
        private void BtnCredits_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder CreditsSB = new StringBuilder();
            CreditsSB.AppendLine("PXL 1GRPRO-C --- Werkplekleren 1 --- Project C#");
            CreditsSB.AppendLine("");
            CreditsSB.AppendLine("");
            CreditsSB.AppendLine("Code en design door Ruben Kog");
            CreditsSB.AppendLine("");
            CreditsSB.AppendLine("Kaarten door www.improvemagic.com");            
            CreditsSB.AppendLine("");
            CreditsSB.AppendLine("Alle overige afbeeldingen gegenereert via Stable Diffusion");
            CreditsSB.AppendLine("");
            CreditsSB.AppendLine("Muziek door Waterflame: Groovy Tower (Royalty-Free)");            
            string allCredits = CreditsSB.ToString();
            CreditsSB.Clear();
            MessageBox.Show(allCredits, "Credits");


        }
        /// <summary>
        /// Window_Closing voorkomt dat de speler het spel per ongeluk afsluit
        /// </summary>
        /// <param name="sender">Dit is eender welke manier waarop de speler het programma wilt afsluiten.</param>
        /// <param name="e">Dit is nodig om het sluiten te verhinderen via e.cancel</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Bent u zeker dat u wilt stoppen met spelen?", "Afsluiten", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }

        }
        /// <summary>
        /// BtnExit_Click sluit het spel af.
        /// </summary>
        /// <param name="sender">Dit is BtnExit.</param>       
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
