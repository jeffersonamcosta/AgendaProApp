using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace AgendaProApp
{
    public partial class Principal : Window
    {
        
        private APIAgendaPro api;
        

        public Principal(APIAgendaPro api)
        {
            InitializeComponent();
            this.api = api;
            
        }


    }
}
