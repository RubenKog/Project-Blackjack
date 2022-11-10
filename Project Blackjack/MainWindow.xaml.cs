using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Project_Blackjack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //GLobale declaraties:

        //Stringbuilders:
        StringBuilder sbS = new StringBuilder();
        StringBuilder sbB = new StringBuilder();


        //Nodig voor kaartgeneratie:
        Random rnd = new Random();
        int rndType;
        int rndWaarde;
        string kaartType;
        string kaartWaarde;

        //Score bijhouden
        int kaartScore;
        int scoreSpeler = 0;
        int scoreBank = 0;

        //Speler of bank een kaart geven?
        bool isSpeler;

        //Gamronde afgelopen?
        bool rondeVoltooid = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnDeel_Click(object sender, RoutedEventArgs e)
        {
            if (rondeVoltooid == false)
            {
                isSpeler = true;
                Geef_Kaart();
                Geef_Kaart();
                isSpeler = false;
                Geef_Kaart();
                BtnDeel.IsEnabled = false;
                BtnHit.IsEnabled = true;
                BtnStand.IsEnabled = true;
            }
            if (rondeVoltooid==true)
            {
                Gameronde_Reset();
                BtnDeel.Content = "Delen";
                rondeVoltooid=false;
            }
            

        }
       
        private async void Geef_Kaart()
        {
            
            if (isSpeler == true)
            {

                Kaart_Trekken();
                sbS.AppendLine($"{kaartType} {kaartWaarde}");
                scoreSpeler = scoreSpeler + kaartScore;
                LijstSpeler.Text = sbS.ToString();
                TxtSScore.Content = scoreSpeler.ToString();
            }
            else if (isSpeler == false)
            {
                Kaart_Trekken();
                sbB.AppendLine($"{kaartType} {kaartWaarde}");
                scoreBank = scoreBank + kaartScore;
                LijstBank.Text = sbB.ToString();
                TxtBScore.Content = scoreBank.ToString();
            }
        }




        private void BtnHit_Click(object sender, RoutedEventArgs e)
        {
            isSpeler = true;
            Geef_Kaart();

            if (scoreSpeler >= 21)
            {
                Game_Einde();
            }



        }

        private void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            isSpeler = false;
            while (scoreBank <= 16)
            { Geef_Kaart(); }

            if (scoreBank >= 16)
            {
                Game_Einde();
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
                    TxtStatus.Content = "Gewonnen";
                }
                else if (scoreSpeler > scoreBank)
                {
                    TxtStatus.Content = "Gewonnen";
                }
                else if (scoreSpeler < scoreBank)
                {
                    TxtStatus.Content = "Verloren";
                }
                else if(scoreSpeler == scoreBank)
                {
                    TxtStatus.Content = "Gelijkspel";
                }

            }

            else if (scoreSpeler > 21)
            {
                TxtStatus.Content = "Verloren";

            }
            else if (scoreSpeler == 21)
            {
                TxtStatus.Content = "Gewonnen";

            }



        }

        private void Gameronde_Reset()
        {
            if (rondeVoltooid == true)
            {
                TxtStatus.Content = "";
                TxtBScore.Content = "";
                TxtSScore.Content = "";
                scoreSpeler = 0;
                scoreBank = 0;
                LijstBank.Text = "";
                LijstSpeler.Text = "";

                sbS.Clear();
                sbB.Clear();


            }
        }

        private void Kaart_Trekken()
        {
            //Generatie van kaarttype en -waarde.
            rndType = rnd.Next(0, 4);
            rndWaarde = rnd.Next(1, 14);
            
            //Interpretatie van type en waarde
            if (rndType==0)
            {
                kaartType = "Harten";
            }
            else if (rndType==1)
            {
                kaartType = "Ruiten";
            }
            else if (rndType==2)
            {
                kaartType = "Schoppen";
            }
            else if (rndType==3)
            {
                kaartType = "Klaveren";
            }

            if (rndWaarde<11)
            {
                kaartWaarde = $"{rndWaarde}";
                kaartScore = rndWaarde;
            }
            else if (rndWaarde>10)
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



        



    }



}
