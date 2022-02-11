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

namespace WinAppMediaPlayerVersie2
{
    public partial class frmServerMediaPlayer : Form
    {
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

        private void btnStartPlay_Click(object sender, EventArgs e)
        {
            Player.controls.play();
            tssMediaPlayer.Text = "Mediaplayer speelt af";
            tssMediaPlayer.ForeColor = Color.Green;
        }       
    }
}
