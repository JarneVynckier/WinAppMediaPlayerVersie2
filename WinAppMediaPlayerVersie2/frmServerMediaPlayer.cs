using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//toevoegen
using System.IO;
//extra voor afspelen muziek na import mediaplayer in toolbox
using WMPLib;
using System.Net.Sockets; //TCPClient en TCPServer verbinden
using System.Net; //IPadressen gebruiken


namespace WinAppMediaPlayerVersie2
{
    public partial class frmServerMediaPlayer : Form
    {
        TcpListener listener; //wachten op client
        TcpClient client; //verbinden met Client
        StreamReader Reader; //berichten ontvangen
        StreamWriter Writer; //berichten versturen
        public frmServerMediaPlayer()
        {
            InitializeComponent();
        }

        private void frmServerMediaPlayer_Shown(object sender, EventArgs e)
        {
            pnlAlleSongs.Width = (splitContMediaPlayer.Panel1.Width - pnlKnoppen.Width) / 2;
        }
        string Pad, SongName;
        private void frmServerMediaPlayer_Load(object sender, EventArgs e)
        {
            string pad = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "muziek");
            if (!Directory.Exists(pad))
            {
                Directory.CreateDirectory(pad);
            }
            foreach (string file in Directory.GetFiles(pad))
            {
                // voeg toe aan resultaat
                if(file.Contains(".mp3"))
                {
                    lstAlleSongs.Items.Add(Path.GetFileNameWithoutExtension(file));
                }               
            }
            //afspeellijst aanmaken
            Player.currentPlaylist = Player.newPlaylist("Klas", "");
            padMuziek = pad;
        }
        string padMuziek;
        private void btnVoegSongToe_Click(object sender, EventArgs e)
        {
            if (ofdZoekSong.ShowDialog() == DialogResult.OK)
            {
                Pad = ofdZoekSong.FileName;
                SongName= Path.GetFileNameWithoutExtension(ofdZoekSong.FileName);
            }
            else { return; }
            if (lstAlleSongs.Items.Contains(SongName)) { MessageBox.Show("Deze song bestaat al!"); return; }
            if (!File.Exists(Pad)) { MessageBox.Show("Zoek een Song!"); return; }
            string pad = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "muziek");
            if (!Directory.Exists(pad))
            {
                Directory.CreateDirectory(pad);                
            }
            //bestand kopiëren naar map muziek             
            File.Copy(Pad, pad + "\\" + SongName + ".mp3");
            //toevoegen aan lijst
            lstAlleSongs.Items.Add( SongName );
        }
        WindowsMediaPlayer Player = new WindowsMediaPlayer();

        private void btnStopPlay_Click(object sender, EventArgs e)
        {
            Player.controls.stop();
            tssMediaPlayer.Text = "Mediaplayer gestopt";
            tssMediaPlayer.ForeColor = Color.Red;
        }

        private void btnVoegToePlayList_Click(object sender, EventArgs e)
        {
            if (lstAlleSongs.SelectedIndex == -1) return; //niets geselecteerd
            if (lstPlaylistSongs.Items.Contains(lstAlleSongs.SelectedItem.ToString())) { MessageBox.Show("Deze song bestaat al!"); return; }
            lstPlaylistSongs.Items.Add(lstAlleSongs.SelectedItem);
            //toevoegen aan PlayList
            Player.currentPlaylist.appendItem(Player.newMedia(padMuziek + "\\" + lstAlleSongs.SelectedItem.ToString() + ".mp3"));
        }
        private void btnVerwijderPlayList_Click(object sender, EventArgs e)
        {
            if (lstPlaylistSongs.SelectedIndex == -1) return; //niets geselecteerd
            int selectie = lstPlaylistSongs.SelectedIndex;
            lstPlaylistSongs.Items.Remove(lstPlaylistSongs.SelectedItem);
            //verwijderen uit Playlist
            IWMPMedia listItem = Player.currentPlaylist.get_Item(selectie);
            Player.currentPlaylist.removeItem(listItem);
            if(lstPlaylistSongs.Items.Count==0)
            {
                tssMediaPlayer.Text = "Mediaplayer gestopt";
                tssMediaPlayer.ForeColor = Color.Red;
            }
        }

        private void chkbStartStopServer_CheckedChanged(object sender, EventArgs e)
        {
            if (chkbStartStopServer.Checked)//knop is ingedrukt
            {
                //controle IP-adres
                IPAddress ipadres;
                int poortNr;
                if (!IPAddress.TryParse(mtxtIPadres.Text.Replace("", ""), out ipadres))
                {
                    txtMelding.AppendText("Ongeldig IP-Adres!\r\n");
                    mtxtIPadres.Focus();
                    return;
                }
                if (!int.TryParse(mtxtPoortnr.Text, out poortNr))
                {
                    txtMelding.AppendText("ongeldig poortnummer!\r\n");
                    mtxtPoortnr.Focus();
                    return;
                }
                //listener opzetten
                try
                {
                    listener = new TcpListener(ipadres, poortNr);
                    listener.Start();
                    //backgroundworker starten
                    bgWorkerListener.WorkerSupportsCancellation = true;
                    bgWorkerListener.RunWorkerAsync();
                    txtMelding.AppendText("Server opgestart\r\n");
                    chkbStartStopServer.Text = "Stop Server";
                    tssTCPServer.Text = "TCP/IP sever gestart";
                    tssTCPServer.ForeColor = Color.Green;
                }
                catch (Exception)
                {
                    txtMelding.AppendText("Kan geen verbinding maken!\r\n");
                }
            }
            else //server stoppen
            {
                if (client != null && client.Connected)
                {
                    //eerst client stopbericht sturen
                    Writer.WriteLine("Disconnect");
                    bgWorkerOntvang.CancelAsync();
                }
                try //sever stoppen
                {
                    if (listener != null)
                    {
                        if (client != null && client.Connected) client.Close();
                        listener.Stop();
                    }
                    chkbStartStopServer.Text = "Start Server";
                    txtMelding.AppendText("Server gestopt!\r\n");
                }
                catch
                {
                    txtMelding.AppendText("Server kan niet gestopt worden.\r\n");
                }
            }
        }

        private void btnStartPlay_Click(object sender, EventArgs e)
        {
            Player.controls.play();
            tssMediaPlayer.Text = "Mediaplayer speelt af";
            tssMediaPlayer.ForeColor = Color.Green;
        }  
        
    }
}
