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
        //GLobale declaraties:


        //Nodig voor kaartgeneratie:
        //kaartCode is nodig voor identificatie in afbeeldingen en arrays.
        Random rnd = new Random();
        int rndType;
        int rndWaarde;
        int kaartCode;
        string VerborgenType;
        string VerborgenWaarde;
        int aantalKaartSpeler = 0;
        int aantalKaartBank = 0;
        int aantalKaartTotaal = 0;

        string kaartType;
        string kaartWaarde;



        //Arrays: 
        int[] alGetrokkenSpeler = new int[11];
        int[] alGetrokkenBank = new int[11];
        int[] alGetrokkenGame = new int[52];

        //Kapitaal (Inzet en budget)
        bool customKapitaal = false;
        float spelerInzet;
        float spelerBudget = 0;

        //Score bijhouden
        int kaartScore;
        int scoreSpeler = 0;
        int scoreBank = 0;

        //Aantal azen
        int aasSpeler = 0;
        int aasBank = 0;

        //Speler of bank een kaart geven?
        bool isSpeler = true;

        //Kaart geselecteerd
        bool uitLijstBank = false;
        bool uitLijstSpeler = false;
        bool uitAutoLijst = false;
        int lijstIndex;
        bool BankVerborgenKaart = false;
        bool AutoCardRotated = false;
        bool DraaiKaart = false;
        bool Drankje = false;

        //Muziek
        SoundPlayer musicPlayer = new SoundPlayer(Properties.Resources.Music1);
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

        private void Klok_Ticked(object sender, EventArgs e)
        {
            TxtKlok.Content = $"Tijd: {DateTime.Now.ToLongTimeString()}";


        }

        private async void BtnDeel_Click(object sender, RoutedEventArgs e)
        { 
            BtnReset.IsEnabled = false;
            

            //Delen bij start van spel
            if (rondeVoltooid == false)
            {

                //Indien budget = 0, geef nieuw budget
                while (spelerBudget == 0)
                {
                    TxtGeld.Content = $"Budget = {spelerBudget}";
                    if (customKapitaal == true)
                    {
                        Start_Kapitaal();

                    }
                    else if (customKapitaal == false)
                    {
                        spelerBudget = 100;
                    }
                    TxtGeld.Content = $"Budget = {spelerBudget}";
                }

                //Laat speler geld inzetten
                Inzet();

                //Start delen
                DraaiKaart = false;
                BtnDeel.IsEnabled = false;
                RondeCounter++;
                isSpeler = true;
                Geef_Kaart();
                await Task.Delay(500);
                Geef_Kaart();
                while (scoreSpeler > 21 && aasSpeler > 0)
                {
                    scoreSpeler -= 10;
                    aasSpeler--;

                }

                TxtSScore.Content = scoreSpeler.ToString();
                await Task.Delay(500);
                isSpeler = false;
                Geef_Kaart();
                BankVerborgenKaart = true;
                await Task.Delay(500);
                Geef_Kaart();
                BtnHit.IsEnabled = true;
                BtnStand.IsEnabled = true;
                if (spelerBudget >= (spelerInzet))
                {
                    BtnDubbel.IsEnabled = true;
                }



            }
            //Deelknop is resetknop bij einde van spelronde
            if (rondeVoltooid == true)
            {
                Gameronde_Reset();
                
            }
            //Indien speler wint met de eerste 2 kaarten moet dit onmiddelijk herkend worden
            if (scoreSpeler == 21)
            {
                BankVerborgenKaart = false;
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                TxtBScore.Content = scoreBank.ToString();
                LijstBank.SelectedIndex = 1;
                Game_Einde();
            }
            if (scoreSpeler != 21 && scoreBank == 21 && BankVerborgenKaart == true)
            {
                BankVerborgenKaart = false;
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                TxtBScore.Content = scoreBank.ToString();
                Drankje = true;
                Afbeelding_Wijzigen();
                MessageBox.Show($"De bank behaalde een BlackJack tijdens het delen, als troost krijg je een drankje van het huis.", "Verfrissend!");
                Game_Einde();


            }
            BtnReset.IsEnabled = true;
        }
        private async void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            isSpeler = true;
            await Task.Delay(250);
            Geef_Kaart();

            while (scoreSpeler > 21 && aasSpeler > 0)
            {
                scoreSpeler -= 10;
                aasSpeler--;

            }
            TxtSScore.Content = scoreSpeler.ToString();

            if (scoreSpeler >= 21)
            {
                await Task.Delay(500);
                BankVerborgenKaart = false;
                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                TxtBScore.Content = scoreBank.ToString();
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
        private async void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnHit.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            isSpeler = false;
            //GeefAanBank is noodzakelijk: Anders stopt bank met kaarten krijgen na overschrijden van 21 met een aas in bezit
            bool geefAanBank = true;
            while (geefAanBank == true)
            {
                if (BankVerborgenKaart == true)
                {

                    BankVerborgenKaart = false;
                    LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                    TxtBScore.Content = scoreBank.ToString();
                    LijstBank.SelectedIndex = 1;


                }
                while (scoreBank < 17)
                {
                    await Task.Delay(500);
                    Geef_Kaart();
                }

                while (scoreBank > 21 && aasBank > 0)
                {
                    scoreBank -= 10;
                    aasBank--;

                }
                TxtBScore.Content = scoreBank.ToString();
                if (scoreBank >= 17)
                {
                    geefAanBank = false;
                }



            }

            if (scoreBank >= 17)
            {
                Game_Einde();
            }
            BtnReset.IsEnabled = true;

        }
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



        private void BtnKapitaal_Used(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            if (customKapitaal == true)
            {
                customKapitaal = false;
            }
            else if (customKapitaal == false)
            {
                customKapitaal = true;
                if (spelerBudget > 0)
                {
                    MessageBox.Show($"Het spel is al bezig. Deze instelling zal activeren bij het starten van een nieuw spel.", "Spel al bezig");
                }
            }
            BtnReset.IsEnabled = true;
        }
        private void Start_Kapitaal()
        {
            bool BudgetOK = false;

            while (BudgetOK == false)
            {
                BudgetOK = float.TryParse(Interaction.InputBox("Geef startkapitaal (afronding naar beneden)", "Invoer", ""), out spelerBudget);
                if (BudgetOK == false)
                {
                    MessageBox.Show("Dat is een foute of te grote invoer, probeer opnieuw door een getal in te voeren.", "Foutieve invoer");
                }
                if (BudgetOK == true && spelerBudget <= 0)
                {
                    MessageBox.Show("Je budget moet groter zijn dan 0.", "Foutieve invoer");
                    BudgetOK = false;
                }
            }
            if (spelerBudget >= 1)
            {
                spelerBudget = Convert.ToSingle(Math.Floor(spelerBudget));
            }
            if (spelerBudget < 1)
            {
                MessageBox.Show($"Je hebt een lekker drankje gekocht met je ${spelerBudget} want dit was te laag om in te zetten.", "Verfrissend!");
                spelerBudget = 0;
                Drankje = true;
                Afbeelding_Wijzigen();
            }
        }
        private void Inzet()
        {
            bool InzetOK = false;
            while (InzetOK == false)
            {
                InzetOK = float.TryParse(Interaction.InputBox("Geef inzet (afronding naar boven).", "Geef inzet", ""), out spelerInzet);
                if (InzetOK == false)
                {
                    MessageBox.Show("Dat is een foute invoer, geef een getal.", "Foutieve invoer");
                }
                if (InzetOK == true && spelerInzet <= 0)
                {
                    MessageBox.Show("De inzet moet groter zijn dan 0.", "Foutieve invoer");
                    InzetOK = false;

                }
                if (InzetOK == true && spelerBudget < spelerInzet)
                {
                    MessageBox.Show($"Je budget is maar {spelerBudget}, gelieve je inzet te verlagen.", "Foutieve invoer");
                    InzetOK = false;

                }
                if (InzetOK == true && spelerInzet < (Convert.ToSingle(Math.Ceiling(spelerBudget / 10))))
                {
                    MessageBox.Show($"Je inzet is maar ${spelerInzet}, je moet minstens ${Math.Ceiling(spelerBudget / 10)} inzetten om te spelen.", "Foutieve invoer");
                    InzetOK = false;
                }
            }

            spelerInzet = Convert.ToSingle(Math.Ceiling(spelerInzet));
            TxtGeld.Content = $"Budget = {spelerBudget} (-{spelerInzet})";
            RondeBudget = spelerBudget;
            spelerBudget -= spelerInzet;
        }
        private void Geef_Kaart()
        {
            //Controle of de kaart gegeven moet worden aan speler of bank
            //Updaten van de kaartlijsten en score
            //Bijhouden van aantal getrokken azen
            //Controle of kaart al getrokken is
            //afbeelding wijzigen




            if (isSpeler == true)
            {
                Kaart_Controleren();
                if (rndWaarde == 1)
                {
                    aasSpeler++;
                }

                alGetrokkenSpeler[aantalKaartSpeler] = kaartCode;
                aantalKaartSpeler++;
                alGetrokkenGame[aantalKaartTotaal] = kaartCode;
                aantalKaartTotaal++;
                scoreSpeler += kaartScore;
                LijstSpeler.Items.Add($"{kaartType} {kaartWaarde}");
                TxtAantalKaarten.Content = $"Aantal kaarten over: {52 - aantalKaartTotaal}";
                TxtSScore.Content = scoreSpeler.ToString();


            }
            else if (isSpeler == false)
            {
                Kaart_Controleren();
                if (rndWaarde == 1)
                {
                    aasBank++;
                }
                alGetrokkenBank[aantalKaartBank] = kaartCode;
                aantalKaartBank++;
                alGetrokkenGame[aantalKaartTotaal] = kaartCode;
                aantalKaartTotaal++;
                TxtAantalKaarten.Content = $"Aantal kaarten over: {52 - aantalKaartTotaal}";

                scoreBank += kaartScore;
                if (BankVerborgenKaart == false)
                {
                    LijstBank.Items.Add($"{kaartType} {kaartWaarde}");
                    TxtBScore.Content = scoreBank.ToString();
                }
                else
                {
                    LijstBank.Items.Add($"Verborgen");

                }
            }
            uitAutoLijst = true;
            if (BankVerborgenKaart == false || isSpeler == true || scoreBank >= 21)
            {
                Afbeelding_Wijzigen();
            }

        }

        private void Kaart_Controleren()
        {
            bool kaartAlGetrokken = true;

            while (kaartAlGetrokken == true)
            {
                Kaart_Trekken();
                kaartAlGetrokken = false;
                for (int i = 0; i < 11; i++)
                {
                    if (kaartCode == alGetrokkenSpeler[i])
                    {
                        kaartAlGetrokken = true;
                    }
                    if (kaartCode == alGetrokkenBank[i])
                    {
                        kaartAlGetrokken = true;
                    }
                }
                foreach (int waarde in alGetrokkenGame)
                {
                    if (kaartCode == waarde)
                    {
                        kaartAlGetrokken = true;
                    }
                }
            }


        }


        private void Kaart_Trekken()
        {
            if (aantalKaartTotaal == 52)
            {

                Drankje = true;
                Afbeelding_Wijzigen();
                TxtAantalKaarten.Content = $"Aantal kaarten over: {52 - aantalKaartTotaal}";
                MessageBox.Show("Alle kaarten zijn gespeeld. Het deck wordt opnieuw geshuffled. Eventuele kaarten al op de tafel blijven daar tot het einde van de ronde. Geniet ondertussen van een drankje.", "Shuffle ");
                Array.Clear(alGetrokkenGame, 0, alGetrokkenGame.Length);
                aantalKaartTotaal = 0;



            }
            //Generatie van kaarttype en -waarde.
            rndType = rnd.Next(1, 5);
            rndWaarde = rnd.Next(1, 14);
            kaartCode = (rndType * 100) + rndWaarde;

            //Interpretatie van type en waarde
            if (rndType == 1)
            {
                kaartType = "Harten";
            }
            else if (rndType == 2)
            {
                kaartType = "Ruiten";
            }
            else if (rndType == 3)
            {
                kaartType = "Schoppen";
            }
            else if (rndType == 4)
            {
                kaartType = "Klaveren";
            }

            if (rndWaarde == 1)
            {
                kaartWaarde = "aas (1 of 11)";
                kaartScore = 11;
            }

            if (rndWaarde < 11 && rndWaarde > 1)
            {
                kaartWaarde = $"{rndWaarde}";
                kaartScore = rndWaarde;
            }
            else if (rndWaarde > 10)
            {

                kaartScore = 10;
                if (rndWaarde == 11)
                {
                    kaartScore = 10;
                    kaartWaarde = "boer (10)";
                }
                else if (rndWaarde == 12)
                {
                    kaartScore = 10;
                    kaartWaarde = "dame (10)";
                }
                else if (rndWaarde == 13)
                {
                    kaartScore = 10;
                    kaartWaarde = "koning (10)";
                }

            }
            if (BankVerborgenKaart == true && isSpeler == false)
            {
                VerborgenType = kaartType;
                VerborgenWaarde = kaartWaarde;

            }


        }
        private void Game_Einde()
        {
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            BtnDeel.IsEnabled = true;
            rondeVoltooid = true;
            BtnDeel.Content = "Nieuwe Ronde";

            if (scoreSpeler < 21)
            {
                if (scoreBank > 21)
                {
                    Game_Gewonnen();
                }
                else if (scoreSpeler > scoreBank)
                {
                    Game_Gewonnen();
                }
                else if (scoreSpeler < scoreBank)
                {
                    Game_Verloren();
                }
                else if (scoreSpeler == scoreBank)
                {
                    Game_Push();
                }

            }


            else if (scoreSpeler > 21)
            {
                Game_Verloren();
            }
            else if (scoreSpeler == 21)
            {
                TxtBScore.Content = scoreBank.ToString();
                MessageBox.Show($"Je behaalde een BlackJack!", "BlackJack!");
                Game_Gewonnen();
            }



        }
        private void Game_Gewonnen()
        {
            TxtStatus.Content = "Gewonnen";
            TxtStatus.Foreground = Brushes.Green;
            if (scoreSpeler == 21)
            {
                spelerBudget = spelerBudget + ((Convert.ToSingle(5.0 / 2.0) * spelerInzet));


            }

            else if (scoreSpeler < 21)
            {
                spelerBudget = spelerBudget + (2 * spelerInzet);

            }
            TxtGeld.Content = $"Budget = {spelerBudget}";
        }
        private void Game_Verloren()
        {
            TxtStatus.Content = "Verloren";
            TxtStatus.Foreground = Brushes.Red;
            TxtGeld.Content = $"Budget = {spelerBudget}";

        }
        private void Game_Push()
        {
            LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
            TxtStatus.Content = "Push";
            spelerBudget += spelerInzet;
            TxtGeld.Content = $"Budget = {spelerBudget}";

        }
        private void Gameronde_Reset()
        {
            if (rondeVoltooid == true)
            {

                HistoriekTekst = $"{spelerBudget - RondeBudget} - {scoreSpeler} / {scoreBank}";
                TxtHistoriek.Content = $"Historiek: {HistoriekTekst}";
                HistoriekTekst = $"{RondeCounter}: {spelerBudget - RondeBudget} - {scoreSpeler} / {scoreBank}";
                HistoriekList.Add(HistoriekTekst);                
                HistoriekList.Reverse();
                HistoriekSB.Clear();
                for(int i = 0; i < HistoriekList.Count; i++)
                {
                    HistoriekSB.AppendLine($"{HistoriekList[i]}");

                }
                HistoriekList.Reverse();


                TxtHistoriek.Content = $"Historiek: {HistoriekTekst}";

                RondeData_Reset();


            }
            if (spelerBudget < 1)
            {
                if (spelerBudget == 0)
                {
                    Drankje = true;
                    Afbeelding_Wijzigen();
                    MessageBox.Show($"Je hebt helaas geen geld meer, maar je krijgt een drankje van het huis!", "Verfrissend!");
                    TxtGeld.Content = $"Budget = {spelerBudget}";

                }
                if (spelerBudget > 0)
                {
                    Drankje = true;
                    Afbeelding_Wijzigen();
                    MessageBox.Show($"Je hebt nog maar ${spelerBudget} over. Te weinig om verder te spelen. Gelukkig zijn de drankjes goedkoop.", "Verfrissend!");
                    spelerBudget = 0;
                    TxtGeld.Content = $"Budget = {spelerBudget}";

                }



            }
        }
        private void AllData_Reset()
        {
            TxtGeld.Content = "Budget: ---";
            TxtAantalKaarten.Content = "Aantal Kaarten in spel: 52";
            BtnHit.IsEnabled = false;
            BtnReset.IsEnabled = false;
            BtnStand.IsEnabled = false;
            RondeData_Reset();
            RondeBudget = 0;
            RondeCounter = 0;
            spelerBudget = 0;
            aantalKaartTotaal = 0;
            Array.Clear(alGetrokkenGame, 0, alGetrokkenGame.Length);          
            HistoriekTekst = "";
            TxtHistoriek.Content = "Historiek: .. - .. / ..";
            HistoriekList.Clear();
            aasSpeler = 0;
            aasBank = 0;
            uitAutoLijst =false;
            uitLijstSpeler=false;
            uitLijstBank=false;
            BankVerborgenKaart = false;
            AutoCardRotated = false;
            DraaiKaart = false;
            Drankje = false;
            rondeVoltooid = false;
            BtnDeel.IsEnabled = true;




        }

        private void RondeData_Reset()
        {
            AutoCardRotated = false;
            BankVerborgenKaart = false;

            TxtStatus.Foreground = Brushes.Black;
            TxtStatus.Content = "";
            TxtBScore.Content = "0";
            TxtSScore.Content = "0";
            scoreSpeler = 0;
            scoreBank = 0;
            LijstBank.Items.Clear();
            LijstSpeler.Items.Clear();
            aasSpeler = 0;
            aasBank = 0;
            spelerInzet = 0;
            for (int i = 0; i < 11; i++)
            {
                alGetrokkenBank[i] = 0;
                alGetrokkenSpeler[i] = 0;

            }
            aantalKaartSpeler = 0;
            aantalKaartBank = 0;
            BtnDeel.Content = "Delen";
            rondeVoltooid = false;

        }

        private void LijstSpeler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LijstSpeler.SelectedIndex > -1)
            {
                uitLijstBank = false;
                uitLijstSpeler = true;
                lijstIndex = LijstSpeler.SelectedIndex;
                Afbeelding_Wijzigen();
                LijstSpeler.SelectedIndex = -1;
            }
        }

        private void LijstBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LijstBank.SelectedIndex > -1)
            {
                if (((LijstBank.SelectedItem)).ToString() != "Verborgen")
                {
                    uitLijstSpeler = false;
                    uitLijstBank = true;
                    lijstIndex = LijstBank.SelectedIndex;
                    Afbeelding_Wijzigen();
                }
                LijstBank.SelectedIndex = -1;
            }

        }

        private void Afbeelding_Wijzigen()
        {

            int arrayWaarde = 0;
            bool rotatieKaart = false;
            if (uitLijstSpeler == true)
            {
                arrayWaarde = alGetrokkenSpeler[lijstIndex];
                uitLijstSpeler = false;
                if (lijstIndex == 2 && DraaiKaart == true)
                {
                    rotatieKaart = true;
                }

            }
            else if (uitLijstBank == true)
            {
                arrayWaarde = alGetrokkenBank[lijstIndex];
                uitLijstBank = false;
                rotatieKaart = false;

            }
            else if (uitAutoLijst == true)
            {
                arrayWaarde = kaartCode;
                uitAutoLijst = false;
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

        private async void BtnDubbel_Click(object sender, RoutedEventArgs e)
        {
            spelerBudget -= spelerInzet;
            spelerInzet *= 2;
            TxtGeld.Content = $"Budget = {RondeBudget} (-{spelerInzet})";
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDubbel.IsEnabled = false;
            DraaiKaart = true;

            isSpeler = true;

            Geef_Kaart();

            while (scoreSpeler > 21 && aasSpeler > 0)
            {
                scoreSpeler -= 10;
                aasSpeler--;

            }
            TxtSScore.Content = scoreSpeler.ToString();
            BtnHit.IsEnabled = true;
            BtnStand.IsEnabled = true;
            await Task.Delay(500);
            if (scoreSpeler >= 21)
            {

                LijstBank.Items[1] = $"{VerborgenType} {VerborgenWaarde}";
                LijstBank.SelectedIndex = 1;

                Game_Einde();
            }
            else
            {

                LijstBank.SelectedIndex = 1;
                TxtBScore.Content = scoreBank.ToString();

                BtnStand_Click(sender, e);
            }
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bent u zeker? Uw budget zal resetten, de historiek zal gewist worden en het deck zal opnieuw geschud worden.", "Herstarten", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {                               
                AllData_Reset();

            }


        }

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

        
    }



}
