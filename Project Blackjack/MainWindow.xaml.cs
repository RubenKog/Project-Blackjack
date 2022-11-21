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

namespace Project_Blackjack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //GLobale declaraties:

        //Stringbuilders: S = Speler, B = Bank
        StringBuilder sbS = new StringBuilder();
        StringBuilder sbB = new StringBuilder();


        //Nodig voor kaartgeneratie:
        Random rnd = new Random();
        int rndType;
        int rndWaarde;
        string kaartType;
        string kaartWaarde;

        //Kapitaal (Inzet en budget)
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
        bool isSpeler;

        //Gamronde afgelopen?
        bool rondeVoltooid = false;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private async void BtnDeel_Click(object sender, RoutedEventArgs e)
        {
            

            //Delen bij start van spel
            if (rondeVoltooid == false)
            {
                //Indien budget = 0, geef nieuw budget
                while (spelerBudget == 0)
                {
                    Start_Kapitaal();
                    TxtGeld.Content = $"Budget = {spelerBudget}";
                    
                }

                //Laat speler geld inzetten
                Inzet();

                //Start delen
                BtnDeel.IsEnabled = false;
                
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
                BtnHit.IsEnabled = true;
                BtnStand.IsEnabled = true;

                

            }
            //Deelknop is resetknop bij einde van het spel
            if (rondeVoltooid==true)
            {
                Gameronde_Reset();
                BtnDeel.Content = "Delen";
                rondeVoltooid=false;
            }
            //Indien speler wint met de eerste 2 kaarten moet dit onmiddelijk herkend worden
            if (scoreSpeler == 21)
            {
                Game_Einde();
            }

        }       
        private async void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            isSpeler = true;
            await Task.Delay(250);
            Geef_Kaart();     
            
            while (scoreSpeler > 21 && aasSpeler >0)
            {
                scoreSpeler -= 10;
                aasSpeler--;

            }
            TxtSScore.Content = scoreSpeler.ToString();
            BtnHit.IsEnabled = true;
            BtnStand.IsEnabled = true;
            if (scoreSpeler >= 21)
            {
                Game_Einde();
            }

            
        }
        private async void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            BtnStand.IsEnabled = false;
            BtnHit.IsEnabled = false;
            isSpeler = false;
            //GeefAanBank is noodzakelijk: Anders stopt bank met kaarten krijgen na overschrijden van 21 met een aas in bezit
            bool geefAanBank = true;
            while (geefAanBank == true)
            {

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
                geefAanBank=false;
                }
                    

                
            }

            if (scoreBank >= 17)
            {
                Game_Einde();
            }
            
        }
        private void Start_Kapitaal()
        {
            bool BudgetOK = false;

            while (BudgetOK == false)
            {
                BudgetOK = float.TryParse(Interaction.InputBox("Geef startkapitaal (afronding naar beneden)", "Invoer", ""), out spelerBudget);
                if (BudgetOK == false)
                {
                    MessageBox.Show("Dat is een foute of te grote invoer, probeer opnieuw door een getal in te voeren", "Foutieve invoer");
                }
                if (BudgetOK == true && spelerBudget <= 0)
                {
                    MessageBox.Show("Je budget moet groter zijn dan 0", "Foutieve invoer");
                    BudgetOK = false;

                }

                
            }
            if (spelerBudget >= 1)
            {
                spelerBudget = Convert.ToSingle(Math.Floor(spelerBudget));
            }
            if (spelerBudget < 1)
            {
                MessageBox.Show($"Je hebt een lekker drankje gekocht met je ${spelerBudget} want dit was te laag om in te zetten", "Verfrissend!");
                spelerBudget = 0;
            }

        }
        private void Inzet()
        {
            bool InzetOK = false;
            while (InzetOK == false)
            {
                InzetOK = float.TryParse(Interaction.InputBox("Geef inzet (afronding naar boven)", "Geef inzet", ""), out spelerInzet);
                if (InzetOK == false)
                {
                    MessageBox.Show("Dat is een foute invoer, geef een getal", "Foutieve invoer");
                }
                if (InzetOK == true && spelerInzet <= 0)
                {
                    MessageBox.Show("De inzet moet groter zijn dan 0", "Foutieve invoer");
                    InzetOK = false;

                }
                if (InzetOK == true && spelerBudget < spelerInzet)
                {
                    MessageBox.Show($"Je budget is maar {spelerBudget}, gelieve je inzet te verlagen", "Foutieve invoer");
                    InzetOK = false;

                }
                if (InzetOK == true && spelerInzet < (Convert.ToSingle(Math.Ceiling(spelerBudget / 10))))
                {
                    MessageBox.Show($"Je inzet is maar ${spelerBudget}, je moet minstens ${Math.Ceiling(spelerBudget / 10)} inzetten om te spelen", "Foutieve invoer");
                    InzetOK = false;
                }



            }

            spelerInzet = Convert.ToSingle(Math.Ceiling(spelerInzet));
            TxtGeld.Content = $"Budget = {spelerBudget} (-{spelerInzet})";
            spelerBudget -= spelerInzet;
        }
        private void Geef_Kaart()
        {
            //Controle of de kaart gegeven moet worden aan speler of bank
            //Updaten van de kaartlijsten en score
            //Bijhouden van aantal getrokken azen


            if (isSpeler == true)
            {

                Kaart_Trekken();
                if (rndWaarde == 1)
                {
                    aasSpeler++;
                }


                sbS.AppendLine($"{kaartType} {kaartWaarde}");
                scoreSpeler += kaartScore;
                LijstSpeler.Text = sbS.ToString();
                TxtSScore.Content = scoreSpeler.ToString();
            }
            else if (isSpeler == false)
            {

                Kaart_Trekken();
                if (rndWaarde == 1)
                {
                    aasBank++;
                }
                sbB.AppendLine($"{kaartType} {kaartWaarde}");
                scoreBank += kaartScore;
                LijstBank.Text = sbB.ToString();
                TxtBScore.Content = scoreBank.ToString();
            }
        }
        private void Kaart_Trekken()
        {
            //Generatie van kaarttype en -waarde.
            rndType = rnd.Next(0, 4);
            rndWaarde = rnd.Next(1, 14);

            //Interpretatie van type en waarde
            if (rndType == 0)
            {
                kaartType = "Harten";
            }
            else if (rndType == 1)
            {
                kaartType = "Ruiten";
            }
            else if (rndType == 2)
            {
                kaartType = "Schoppen";
            }
            else if (rndType == 3)
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


        }
        private void Game_Einde()
        {
            BtnHit.IsEnabled = false;
            BtnStand.IsEnabled = false;
            BtnDeel.IsEnabled = true;
            rondeVoltooid = true;
            BtnDeel.Content = "Nieuwe Ronde";

            if (scoreSpeler<21)
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
                else if(scoreSpeler == scoreBank)
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
                Game_Gewonnen();
            }



        }
        private void Game_Gewonnen()
        {
            TxtStatus.Content = "Gewonnen";
            TxtStatus.Foreground = Brushes.Green;
            if (scoreSpeler == 21)
            {
                spelerBudget = spelerBudget + ((Convert.ToSingle(5.0/2.0)*spelerInzet));
                

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
            TxtStatus.Content = "Push";
            spelerBudget += spelerInzet;
            TxtGeld.Content = $"Budget = {spelerBudget}";
        }
        private void Gameronde_Reset()
        {
            if (rondeVoltooid == true)
            {
                TxtStatus.Foreground = Brushes.Black  ;
                TxtStatus.Content = "";
                TxtBScore.Content = "";
                TxtSScore.Content = "";
                scoreSpeler = 0;
                scoreBank = 0;
                LijstBank.Text = "";
                LijstSpeler.Text = "";

                sbS.Clear();
                sbB.Clear();
                aasSpeler = 0;
                aasBank = 0;
                spelerInzet = 0;
                

            }
        }


        



        



    }



}
