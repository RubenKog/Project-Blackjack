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

        
        //Nodig voor kaartgeneratie:
        //kaartCode is nodig voor identificatie in afbeeldingen en arrays.
        Random rnd = new Random();
        int rndType;
        int rndWaarde;
        int kaartCode;
        int aantalKaartSpeler = 0;
        int aantalKaartBank = 0;

        string kaartType;
        string kaartWaarde;
        
        

        //Arrays: 
        int[] alGetrokkenSpeler = new int[11];
        int[] alGetrokkenBank = new int[11];

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
        bool isSpeler;

        //Kaart geselecteerd
        bool uitLijstBank = false;
        bool uitLijstSpeler = false;
        bool uitAutoLijst = false;
        int lijstIndex;



        //Gamronde afgelopen?
        bool rondeVoltooid = false;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private async void BtnDeel_Click(object sender, RoutedEventArgs e)
        {
            //Afbeelding_Wijzigen();

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
        private void BtnKapitaal_Used(object sender, RoutedEventArgs e)
        {
            if (customKapitaal == true)
            {
                customKapitaal = false;
            }
            else if (customKapitaal == false)
            {
                customKapitaal = true;
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
                    MessageBox.Show($"Je inzet is maar ${spelerInzet}, je moet minstens ${Math.Ceiling(spelerBudget / 10)} inzetten om te spelen", "Foutieve invoer");
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
            //Controle of kaart al getrokken is
            //afbeelding wijzigen




            if (isSpeler == true)
            {
                bool kaartAlGetrokken = true;

                while (kaartAlGetrokken == true)
                {
                    Kaart_Trekken();
                    kaartAlGetrokken = false;
                    for (int i = 0; i < 11; i++)
                    {
                        if( kaartCode == alGetrokkenSpeler[i])
                        {
                            kaartAlGetrokken = true;
                        }
                        if (kaartCode == alGetrokkenBank[i])
                        {
                            kaartAlGetrokken = true;
                        }

                    }
                    

                    
                }
                


                if (rndWaarde == 1)
                {
                    aasSpeler++;
                }

                alGetrokkenSpeler[aantalKaartSpeler] = kaartCode;
                aantalKaartSpeler++;
                scoreSpeler += kaartScore;                
                LijstSpeler.Items.Add($"{kaartType} {kaartWaarde}");
                TxtSScore.Content = scoreSpeler.ToString();
                

            }
            else if (isSpeler == false)
            {
                bool kaartAlGetrokken = true;

                while (kaartAlGetrokken == true)
                {
                    Kaart_Trekken();
                    kaartAlGetrokken= false;
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


                }
                if (rndWaarde == 1)
                {
                    aasBank++;
                }
                alGetrokkenBank[aantalKaartBank] = kaartCode;
                aantalKaartBank++;
                scoreBank += kaartScore;
                LijstBank.Items.Add($"{kaartType} {kaartWaarde}");
                TxtBScore.Content = scoreBank.ToString();
            }
            uitAutoLijst = true;
            Afbeelding_Wijzigen();
        }
        private void Kaart_Trekken()
        {
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


            }
            if (spelerBudget < 1)
            {
                if (spelerBudget == 0) 
                {
                    MessageBox.Show($"Je hebt helaas geen geld meer, maar je krijgt een drankje van het huis!", "Verfrissend!");
                    TxtGeld.Content = $"Budget = {spelerBudget}";
                }
                if (spelerBudget > 0)
                {
                    MessageBox.Show($"Je hebt nog maar ${spelerBudget} over. Te weinig om verder te spelen. Gelukkig zijn de drankjes goedkoop.", "Verfrissend!");
                    spelerBudget = 0;
                    TxtGeld.Content = $"Budget = {spelerBudget}";
                }
                    
                    

            }
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
                uitLijstSpeler = false;
                uitLijstBank = true;
                lijstIndex = LijstBank.SelectedIndex;
                Afbeelding_Wijzigen();
                LijstBank.SelectedIndex = -1;
            }

        }

        private void Afbeelding_Wijzigen()
        {
            int arrayWaarde = 0;
            if (uitLijstSpeler == true)
            {
                arrayWaarde = alGetrokkenSpeler[lijstIndex];
                uitLijstSpeler = false;

            }
            else if (uitLijstBank == true)
            {
                arrayWaarde = alGetrokkenBank[lijstIndex];
                uitLijstBank = false;

            }
            else if (uitAutoLijst == true)
            {
                arrayWaarde = kaartCode;
                uitAutoLijst = false;

            }
            
            BitmapImage ImgeKaart = new BitmapImage();
            ImgeKaart.BeginInit();
            ImgeKaart.UriSource = new Uri($"{arrayWaarde}.PNG", UriKind.Relative);
            ImgeKaart.EndInit();
            ImgKaart.Source = ImgeKaart;
            


        }
    }



}
