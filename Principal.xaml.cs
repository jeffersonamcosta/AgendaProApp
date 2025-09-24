using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace AgendaProApp
{
    public partial class Principal : Window
    {
        private static readonly Regex  regexHora = new Regex(@"^[0-9:]+$");
        private APIAgendaPro api;
        private Dictionary<int, JsonElement> _eventos = new();

        public Principal(APIAgendaPro api)
        {
            InitializeComponent();
            this.api = api;
            
        }


    }
}
