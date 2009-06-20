/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/


using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;
using System.Xml;
using System.Threading;


namespace Kinovea.Services
{
	/// <summary>
	/// A class to encapsulate the user's preferences.
	/// There is really two kind of preferences handled here:
	/// - The filesystem independant preferences (language, flags, etc.)
	/// - The filesystem dependant ones. (file history, shortcuts, etc.)
	/// 
	/// File system independant are stored in preferences.xml
	/// others are handled through .NET settings framework.
	/// TODO: homogenize ?
	/// </summary>
    public class PreferencesManager
    {
        #region Satic Properties
        public static string ReleaseVersion
        {
            // This prop is set as the first instruction of the RootKernel ctor.
            get{ return Properties.Settings.Default.Release; }
            set{ Properties.Settings.Default.Release = value;}
        }
        public static string SettingsFolder
        {
        	// Store settings in user space. 
        	// If it doesn't exist, this folder is created at startup.
        	get{ return m_AppdataFolder;}       
        }
        public static ResourceManager ResourceManager
        {
        	// FIXME: all folders should be accessed through real props.
            get { return Properties.Resources.ResourceManager; }
        }
        public static bool ExperimentalRelease
        {
            // Set in RootKernel ctor. Used to show/hide certain menus.
            get{ return Properties.Settings.Default.ExperimentalRelease; }
            set{ Properties.Settings.Default.ExperimentalRelease = value;}
        }
        
        
        #region Native Languages Names
        // We keep the native languages name here.
        // This way they are globally accessible, and no risk to be translated.
        public static string LanguageEnglish
        {
            get { return "English"; }
        }
        public static string LanguageFrench
        {
            get { return "Français"; }
        }
        public static string LanguageDutch
        {
            get { return "Nederlands"; }
        }
        public static string LanguageGerman
        {
            get { return "Deutsch"; }
        }
        public static string LanguageSpanish
        {
            get { return "Español"; }
        }
        public static string LanguagePolish
        {
            get { return "Polski"; }
        }
        public static string LanguageItalian
        {
            get { return "Italiano"; }
        }
        public static string LanguagePortuguese
        {
            get { return "Português"; }
        }
        public static string LanguageRomanian
        {
            get { return "Română"; }
        }
        #endregion
        #endregion
        
        #region Properties (Preferences)
        public int HistoryCount
        {
            get { return m_iFilesToSave; }
            set { m_iFilesToSave = value;}
        }
        public string UILanguage
        {
            get { return m_UILanguage; }
            set { m_UILanguage = value; }
        }
        public TimeCodeFormat TimeCodeFormat
        {
            get { return m_TimeCodeFormat; }
            set { m_TimeCodeFormat = value; }
        }
        public Color GridColor
        {
            get { return m_GridColor; }
            set { m_GridColor = value; }
        }
        public Color Plane3DColor
        {
            get { return m_Plane3DColor; }
            set { m_Plane3DColor = value; }
        }
        public int WorkingZoneSeconds
        {
            get { return m_iWorkingZoneSeconds; }
            set { m_iWorkingZoneSeconds = value; }
        }
        public int WorkingZoneMemory
        {
            get { return m_iWorkingZoneMemory; }
            set { m_iWorkingZoneMemory = value; }
        }
        public InfosFading DefaultFading
        {
            get { return m_DefaultFading; }
            set { m_DefaultFading = value; }
        }
        public bool DrawOnPlay
        {
            get { return m_bDrawOnPlay; }
            set { m_bDrawOnPlay = value; }
        }
        public bool ExplorerVisible
        {
            get { return m_bIsExplorerVisible; }
            set { m_bIsExplorerVisible = value;}
        }
        public int ExplorerSplitterDistance
        {
        	// Splitter between Explorer and ScreenManager
            get { return m_iExplorerSplitterDistance; }
            set { m_iExplorerSplitterDistance = value; }
        }
		public int ExplorerFilesSplitterDistance
        {
        	// Splitter between folders and files on Explorer tab
            get { return m_iExplorerFilesSplitterDistance; }
            set { m_iExplorerFilesSplitterDistance = value; }
        }
		public ExplorerThumbSizes ExplorerThumbsSize
		{
			// Size category of the thumbnails.
            get { return m_iExplorerThumbsSize; }
            set { m_iExplorerThumbsSize = value; }				
		}
		public int ShortcutsFilesSplitterDistance
        {
        	// Splitter between folders and files on Shortcuts tab
            get { return m_iShortcutsFilesSplitterDistance; }
            set { m_iShortcutsFilesSplitterDistance = value; }
        }
        public List<ShortcutFolder> ShortcutFolders
        {
        	// FIXME.
        	// we want the client of the prop to get a read only access.
        	// here we offer a reference on an internal objetc, he can call .Clear().
        	get{ return m_ShortcutFolders;}
        }
        public string LastBrowsedDirectory 
        {
			get 
			{ 
				return Properties.Settings.Default.BrowserDirectory; 
			}
			set 
			{ 
				Properties.Settings.Default.BrowserDirectory = value;
        		Properties.Settings.Default.Save();
			}
		}
        public ActiveFileBrowserTab ActiveTab 
        {
			get { return m_ActiveFileBrowserTab; }
			set { m_ActiveFileBrowserTab = value; }
		}
        
        #endregion

        #region Members
        // Preferences
        private List<string> m_HistoryList = new List<string>();
        private int m_iFilesToSave = 5;
        private string m_UILanguage;
        private TimeCodeFormat m_TimeCodeFormat = TimeCodeFormat.ClassicTime;
        private Color m_GridColor = Color.White;
        private Color m_Plane3DColor = Color.White;
        private int m_iWorkingZoneSeconds = 12;
        private int m_iWorkingZoneMemory = 512;
        private InfosFading m_DefaultFading = new InfosFading();
        private bool m_bDrawOnPlay = true;
        private bool m_bIsExplorerVisible = true;
        private int m_iExplorerSplitterDistance = 250;
        private int m_iExplorerFilesSplitterDistance = 350;
        private int m_iShortcutsFilesSplitterDistance = 350;
        private ExplorerThumbSizes m_iExplorerThumbsSize = ExplorerThumbSizes.Medium; 
        private static string m_AppdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Kinovea\\";
        private List<ShortcutFolder> m_ShortcutFolders = new List<ShortcutFolder>();
        private ActiveFileBrowserTab m_ActiveFileBrowserTab = ActiveFileBrowserTab.Explorer;
        
        // Helpers members
        private static PreferencesManager m_instance = null;
        private ToolStripMenuItem m_HistoryMenu;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor & Singleton
        public static PreferencesManager Instance()
        {
            if (m_instance == null)
            {
                m_instance = new PreferencesManager();
            }
            return m_instance;
        }
        private PreferencesManager()
        {
            // By default we use the System Language.
            // If it is not supported, it will fall back to English.
            m_UILanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            
            Import();
            GetHistoryAsList();
        }
        #endregion

        #region Import/Export Public interface
        public void Export()
        {
            log.Debug("Exporting preferences.");
            FlushToDisk(m_AppdataFolder + Properties.Resources.ResourceManager.GetString("PreferencesFile"));
        }
        public void Import()
        {
            log.Debug("Importing preferences.");
            ParseConfigFile(m_AppdataFolder + Properties.Resources.ResourceManager.GetString("PreferencesFile"));
        }
        #endregion

        #region XML Helpers
        private void FlushToDisk(string filePath)
        {
            try
            {
                XmlTextWriter PreferencesWriter = new XmlTextWriter(filePath, null);
                PreferencesWriter.Formatting = Formatting.Indented;
                PreferencesWriter.WriteStartDocument();
                PreferencesWriter.WriteStartElement("KinoveaPreferences");

                // Format version
                PreferencesWriter.WriteStartElement("FormatVersion");
                PreferencesWriter.WriteString("1.2");
                PreferencesWriter.WriteEndElement();

                // Preferences
                PreferencesWriter.WriteElementString("HistoryCount", m_iFilesToSave.ToString());
                PreferencesWriter.WriteElementString("Language", m_UILanguage);
                PreferencesWriter.WriteElementString("TimeCodeFormat", m_TimeCodeFormat.ToString());
                PreferencesWriter.WriteStartElement("GridColorRGB");
                PreferencesWriter.WriteString(m_GridColor.R.ToString() + ";" + m_GridColor.G.ToString() + ";" + m_GridColor.B.ToString());
                PreferencesWriter.WriteEndElement();
                PreferencesWriter.WriteStartElement("Plane3DColorRGB");
                PreferencesWriter.WriteString(m_Plane3DColor.R.ToString() + ";" + m_Plane3DColor.G.ToString() + ";" + m_Plane3DColor.B.ToString());
                PreferencesWriter.WriteEndElement();
                PreferencesWriter.WriteElementString("WorkingZoneSeconds", m_iWorkingZoneSeconds.ToString());
                PreferencesWriter.WriteElementString("WorkingZoneMemory", m_iWorkingZoneMemory.ToString());

                m_DefaultFading.ToXml(PreferencesWriter, true);
                
                PreferencesWriter.WriteElementString("DrawOnPlay", m_bDrawOnPlay.ToString());
                PreferencesWriter.WriteElementString("ExplorerThumbnailsSize", m_iExplorerThumbsSize.ToString());
                PreferencesWriter.WriteElementString("ExplorerVisible", m_bIsExplorerVisible.ToString());
                PreferencesWriter.WriteElementString("ExplorerSplitterDistance", m_iExplorerSplitterDistance.ToString());
                PreferencesWriter.WriteElementString("ActiveFileBrowserTab", m_ActiveFileBrowserTab.ToString());
                PreferencesWriter.WriteElementString("ExplorerFilesSplitterDistance", m_iExplorerFilesSplitterDistance.ToString());
                PreferencesWriter.WriteElementString("ShortcutsFilesSplitterDistance", m_iShortcutsFilesSplitterDistance.ToString());
                PreferencesWriter.WriteStartElement("Shortcuts");
                foreach(ShortcutFolder sf in m_ShortcutFolders)
                {
                	sf.ToXml(PreferencesWriter);
                }
                PreferencesWriter.WriteEndElement();
                
                PreferencesWriter.WriteEndElement();
                PreferencesWriter.WriteEndDocument();
                PreferencesWriter.Flush();
                PreferencesWriter.Close();
            }
            catch(Exception)
            {
                log.Error("Error happenned while writing preferences.");
            }
        }
        private void ParseConfigFile(string filePath)
        {
            // Fill the local variables with infos found in the XML file.
            XmlReader PreferencesReader = new XmlTextReader(filePath);

            if (PreferencesReader != null)
            {
                try
                {
                    while (PreferencesReader.Read())
                    {
                        if ((PreferencesReader.IsStartElement()) && (PreferencesReader.Name == "KinoveaPreferences"))
                        {
                            while (PreferencesReader.Read())
                            {
                                if (PreferencesReader.IsStartElement())
                                {
                                    switch (PreferencesReader.Name)
                                    {
                                        case "HistoryCount":
                                            m_iFilesToSave = int.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "Language":
                                            m_UILanguage = PreferencesReader.ReadString();
                                            break;
                                        case "TimeCodeFormat":
                                            m_TimeCodeFormat = ParseTimeCodeFormat(PreferencesReader.ReadString());
                                            break;
                                        case "GridColorRGB":
                                            m_GridColor = XmlHelper.ColorParse(PreferencesReader.ReadString(), ';');
                                            break;
                                        case "Plane3DColorRGB":
                                            m_Plane3DColor = XmlHelper.ColorParse(PreferencesReader.ReadString(), ';');
                                            break;
                                        case "WorkingZoneSeconds":
                                            m_iWorkingZoneSeconds = int.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "WorkingZoneMemory":
                                            m_iWorkingZoneMemory = int.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "InfosFading":
                                            m_DefaultFading.FromXml(PreferencesReader);
                                            break;
                                        case "DrawOnPlay":
                                            m_bDrawOnPlay = bool.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "ExplorerThumbnailsSize":
                                            m_iExplorerThumbsSize = (ExplorerThumbSizes)ExplorerThumbSizes.Parse(m_iExplorerThumbsSize.GetType(), PreferencesReader.ReadString());
                                            break;
                                        case "ExplorerVisible":
                                            m_bIsExplorerVisible = bool.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "ExplorerSplitterDistance":
                                            m_iExplorerSplitterDistance = int.Parse(PreferencesReader.ReadString());
                                            break;
										case "ActiveFileBrowserTab":
                                            m_ActiveFileBrowserTab = (ActiveFileBrowserTab)ActiveFileBrowserTab.Parse(m_ActiveFileBrowserTab.GetType(), PreferencesReader.ReadString());
                                            break;
                                        case "ExplorerFilesSplitterDistance":
                                            m_iExplorerFilesSplitterDistance = int.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "ShortcutsFilesSplitterDistance":
                                            m_iShortcutsFilesSplitterDistance = int.Parse(PreferencesReader.ReadString());
                                            break;
                                        case "Shortcuts":
                                            ParseShortcuts(PreferencesReader);
                                            break;
                                        default:
                                            // Preference from a newer file format...
                                            // We don't have a holder variable for it.
                                            break;
                                    }
                                }
                                else if (PreferencesReader.Name == "KinoveaPreferences")
                                {
                                    break;
                                }
                                else
                                {
                                    // Fermeture d'un tag interne.
                                }
                            }
                        }
                    }
                    
                }
                catch (Exception)
                {
                    log.Error("Error happenned while parsing preferences. We'll keep the default values.");
                }
                finally
                {
                    PreferencesReader.Close();
                }
            }
        }
        private TimeCodeFormat ParseTimeCodeFormat(string _format)
        {
            TimeCodeFormat tcf;

            // cannot use a switch, a constant value is expected.

            if(_format.Equals(TimeCodeFormat.ClassicTime.ToString()))
            {
                tcf = TimeCodeFormat.ClassicTime;
            }
            else if (_format.Equals(TimeCodeFormat.Frames.ToString()))
            {
                tcf = TimeCodeFormat.Frames;
            }
            else if (_format.Equals(TimeCodeFormat.TenThousandthOfHours.ToString()))
            {
                tcf = TimeCodeFormat.TenThousandthOfHours;
            }
            else if (_format.Equals(TimeCodeFormat.HundredthOfMinutes.ToString()))
            {
                tcf = TimeCodeFormat.HundredthOfMinutes;
            }
            else if (_format.Equals(TimeCodeFormat.Timestamps.ToString()))
            {
                tcf = TimeCodeFormat.Timestamps;
            }
            else
            {
                // Unkown format. May be a Preferences file from a newer version.
                // We'll stick to default.
                tcf = TimeCodeFormat.ClassicTime;
            }
            
            return tcf;
        }
        private void ParseShortcuts(XmlReader _xmlReader)
        {
        	m_ShortcutFolders.Clear();
        	
        	while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Shortcut")
                    {
                    	ShortcutFolder sf = ShortcutFolder.FromXml(_xmlReader);
                    	if(sf != null)
                    	{
                    		m_ShortcutFolders.Add(sf);
                    	}
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "Shortcuts")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
        	}    	
        }
        #endregion

        #region Local files target.
        public void RegisterHistoryMenu(ToolStripMenuItem historyMenu)
        {
            this.m_HistoryMenu = historyMenu;
        }
        public void OrganizeHistoryMenu()
        {
            // Affichage des sous menus. Seulement le nombre max et seulement si ils sont effectivement renseignés.
            bool bHistoryExists = false;
            int i;

            for (i = 0; i < m_HistoryMenu.DropDownItems.Count-2; i++)
            {
                if ( (m_HistoryList[i].Length > 0) && (i < m_iFilesToSave) )
                {
                    m_HistoryMenu.DropDownItems[i].Text = Path.GetFileName(m_HistoryList[i]);
                    m_HistoryMenu.DropDownItems[i].Visible = true;
                    bHistoryExists = true;
                }
                else
                {
                    m_HistoryMenu.DropDownItems[i].Visible = false;
                }
            }


            // Séparateur et Reset.
            if (bHistoryExists)
            {
                m_HistoryMenu.DropDownItems[m_HistoryMenu.DropDownItems.Count - 2].Visible = true;
                m_HistoryMenu.DropDownItems[m_HistoryMenu.DropDownItems.Count - 1].Visible = true;
                m_HistoryMenu.Enabled = true; 
            }
            else
            {
                m_HistoryMenu.DropDownItems[m_HistoryMenu.DropDownItems.Count - 2].Visible = false;
                m_HistoryMenu.DropDownItems[m_HistoryMenu.DropDownItems.Count - 1].Visible = false;
                m_HistoryMenu.Enabled = false;
            }


        }
        private void GetHistoryAsList()
        {
            // Récupération de l'historique dans une liste pour faciliter la lecture écriture
            m_HistoryList.Clear();

            //Nombre de fichiers à conserver.
            //m_iFilesToSave = Properties.Settings.Default.FilesToSave;
           
            //Liste des fichiers conservés.
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo1);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo2);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo3);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo4);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo5);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo6);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo7);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo8);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo9);
            m_HistoryList.Add(Properties.Settings.Default.HistoryVideo10);
            
        }
        private void PutListAsHistory()
        {
            //Ici les items plus anciens que le nombre max autorisé ont déjà été passé à 'chaîne vide' 

            //Properties.Settings.Default.FilesToSave = m_iFilesToSave;

            Properties.Settings.Default.HistoryVideo1 = m_HistoryList[0];
            Properties.Settings.Default.HistoryVideo2 = m_HistoryList[1];
            Properties.Settings.Default.HistoryVideo3 = m_HistoryList[2];
            Properties.Settings.Default.HistoryVideo4 = m_HistoryList[3];
            Properties.Settings.Default.HistoryVideo5 = m_HistoryList[4];
            Properties.Settings.Default.HistoryVideo6 = m_HistoryList[5];
            Properties.Settings.Default.HistoryVideo7 = m_HistoryList[6];
            Properties.Settings.Default.HistoryVideo8 = m_HistoryList[7];
            Properties.Settings.Default.HistoryVideo9 = m_HistoryList[8];
            Properties.Settings.Default.HistoryVideo10 = m_HistoryList[9];

            Properties.Settings.Default.Save();
        }
        public void HistoryReset()
        {
            for(int i = 0; i<m_HistoryList.Count;i++) 
                m_HistoryList[i] = "";

            PutListAsHistory();
        }
        public void HistoryAdd( string file)
        {
            //------------------------------------------------------------------------
            // Ajoute un fichier à l'historique.
            //------------------------------------------------------------------------
            int     i;
            int     iAlreadyListedAt = -1;


            // Vérifier qu'il n'est pas déjà présent.
            for (i = 0; i < m_iFilesToSave; i++)
            {
                if (m_HistoryList[i] == file)
                {
                    iAlreadyListedAt = i;
                }
            }


            if (iAlreadyListedAt < 0)
            {
                //Décaler par la fin.
                for (i = m_iFilesToSave - 1; i > 0; i--)
                {
                    m_HistoryList[i] = m_HistoryList[i - 1];
                }

                //Renseigner le nouveau
                m_HistoryList[0] = file;

                //commit
                PutListAsHistory();
            }
            else
            {
                // Le fichier est déjà présent.
                // décaler à partir de l'ancien index.

                for (i = iAlreadyListedAt; i > 0; i--)
                {
                    m_HistoryList[i] = m_HistoryList[i - 1];
                }

                //Renseigner le nouveau
                m_HistoryList[0] = file;

                //commit
                PutListAsHistory();
            }
        }
        public string GetFilePathAtIndex(int index)
        {
            return m_HistoryList[index]; 
        }
        #endregion
    }
    
	/// <summary>
	/// Timecode formats.
	/// The preferences combo box must keep this order.
	/// </summary>
    public enum TimeCodeFormat
    {
        ClassicTime,
        Frames,
        TenThousandthOfHours,
        HundredthOfMinutes,
        TimeAndFrames,
        Timestamps,
        Unknown,
        NumberOfTimeCodeFormats
    }
	
	/// <summary>
	/// Last active tab.
	/// Must keep the same ordering as in FileBrowserUI.
	/// </summary>
	public enum ActiveFileBrowserTab
    {
    	Explorer = 0,
    	Shortcuts
    }
	
	/// <summary>
	/// Size of the thumbnails in the explorer.
	/// Sizes are expressed in number of thumbnails that should fit in the width of the explorer.
	/// the actual size of any given thumbnail will change depending on the available space.
	/// </summary>
	public enum ExplorerThumbSizes
	{
		ExtraLarge = 4,
		Large = 5,
		Medium = 7,
		Small = 10,
		ExtraSmall = 14
	};

}
