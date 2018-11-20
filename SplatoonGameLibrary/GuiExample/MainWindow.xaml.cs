using GuiExample.Helpers;
using SplatoonGameLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace GuiExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The object holding all the game play logic
        /// </summary>
        GameBoard Board;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // My window has loaded lets start creating
            // Some GUI stuff
            // Questions for game setup
            var numPlayerPerTeam = PromptDialog.Prompt<int>("Number of players per team", "Game Board Setting");
            var gameLength = PromptDialog.Prompt<int>("Length of game in minutes", "Game Board Setting");
            var gameSize = PromptDialog.Prompt<int>("Size of game board squared", "Game Board Setting");
            
            //Using the size variable create a Grid
            for (int x = 0; x < gameSize; x++)
            {
                // Neat GUI trick to create simple spacers
                BaseGrid.ColumnDefinitions.Add(new ColumnDefinition());
                BaseGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int x = 0; x < gameSize; x++)
            {
                for (int y = 0; y < gameSize; y++)
                {
                    // Create a new grid object within the root grid
                    var g = new Grid();
                    // Set it's location to something for example (0,0)
                    Grid.SetRow(g, x);
                    Grid.SetColumn(g, y);
                    // Store the location as a string in a simple storage property on Grids
                    g.Tag = $"({x},{y})";

                    // Set the background to white
                    g.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));

                    // Append the grid to the GUI
                    BaseGrid.Children.Add(g);
                }
            }

            // Setup the game board
            Board = new GameBoard(gameSize, 2, numPlayerPerTeam, TimeSpan.FromMinutes(gameLength));

            // Register for events to be fired
            Board.OnColorChanged += Board_OnColorChanged;
            Board.OnGameEnd += Board_OnGameEnd;

            // Start the game up
            Board.StartGame();

        }

        private void Board_OnGameEnd(GameBoard sender, IEnumerable<KeyValuePair<Team, int>> Results, Team Winner, int WinnerCount)
        {
            // Simple Testing Output to verify GUI results
            //for (int x = 0; x < 5; x++)
            //{
            //    for (int y = 0; y <5; y++)
            //    {
            //        // Get Square at this location
            //        var sqr = sender.GetFinalBoard().SelectMany(i => i).Where(s => s.X == x && s.Y == y).Single();
            //        if (sqr.CurrentStatus.Team != null)
            //        {
            //            Debug.Write(String.Concat(" ", sqr.CurrentStatus.Team.TeamColor.Identifier, " "));
            //        }
            //        else
            //        {
            //            Debug.Write(String.Concat(" ", 'C', " "));
            //        }
            //    }
            //    Debug.WriteLine(String.Empty);
            //}


            MessageBox.Show($"{Winner.TeamColor.Identifier} has won the game with {WinnerCount} squares!");
        }

        private void Board_OnColorChanged(SplatoonGameLibrary.Color color, int X, int Y)
        {
            // Built-in method of changing GUI from another thread
            Dispatcher.Invoke(()=> 
            {
                // Search pattern to match against Grids
                var search = $"({X},{Y})";
                
                var gridSearch = BaseGrid.Children.Cast<Grid>().Where(x => (x.Tag as String).Equals(search)).Single();

                gridSearch.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
            });
        }
    }
}
