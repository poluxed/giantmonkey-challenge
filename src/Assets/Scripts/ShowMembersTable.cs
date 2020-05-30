using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.Text;

public class ShowMembersTable : MonoBehaviour
{
    //Script variables
    private static Texture2D _defaultTexture;
    private static GUIStyle _defaultStyle;
    private string latestMd5;
    private bool isReadingFile;
    private TeamMembersTable teamsMemberTable;

    //Font styles
    private GUIStyle _smallFont;
    private GUIStyle _headerFont;
    private GUIStyle _titleFont;

    //Colors
    private Color _colorTitle;
    private Color _colorHeadingTable;
    private Color _colorContentTable;

    void OnGUI()
    {
        teamsMemberTable = RetrieveData();
        Render(teamsMemberTable);
    }

    /// <summary>
    /// Render a TeamMembersTable
    /// </summary>
    /// <param name="teamData">Receives the team data to be rendered</param>
    private void Render(TeamMembersTable teamData)
    {
        //set colors
        _colorTitle = new Color(1, 1, 1);
        _colorHeadingTable = new Color(0.8f, 0.8f, 0.8f);
        _colorContentTable = new Color(0.6f, 0.6f, 0.6f);

        //set font styles
        _headerFont = new GUIStyle();
        _titleFont = new GUIStyle();
        _smallFont = new GUIStyle();

        _titleFont.fontSize = 18;
        _titleFont.alignment = TextAnchor.MiddleCenter;
        _headerFont.fontSize = 14;
        _headerFont.alignment = TextAnchor.MiddleCenter;
        _smallFont.fontSize = 10;
        _smallFont.alignment = TextAnchor.MiddleCenter;

        //set dedault texture and default style
        if (_defaultTexture == null)
        {
            _defaultTexture = new Texture2D(1, 1);
        }
        if (_defaultStyle == null)
        {
            _defaultStyle = new GUIStyle();
        }

        //calculate the horizontal space that one cell should use
        var horizontalSpace = Screen.width / teamData.ColumnHeaders.Count + 1;
        //fix height of cells in 30
        var verticalSpace = 30;

        //Draw title
        GUIDrawRect(new Rect(0, 0, Screen.width, verticalSpace * 2), _colorTitle, "Team Members", _titleFont);

        //Draw table headers
        for (int i = 0; i < teamData.ColumnHeaders.Count; i++)
        {
            GUIDrawRect(new Rect(i * horizontalSpace, verticalSpace * 2, horizontalSpace, verticalSpace), _colorHeadingTable, teamData.ColumnHeaders[i], _headerFont);
        }

        //draw table rows (members)
        for (int i = 0; i < teamData.Data.Count; i++)
        {
            GUIDrawRect(new Rect(0 * horizontalSpace, verticalSpace * (i + 3), horizontalSpace, verticalSpace), _colorContentTable, teamData.Data[i].ID, _smallFont);
            GUIDrawRect(new Rect(1 * horizontalSpace, verticalSpace * (i + 3), horizontalSpace, verticalSpace), _colorContentTable, teamData.Data[i].Name, _smallFont);
            GUIDrawRect(new Rect(2 * horizontalSpace, verticalSpace * (i + 3), horizontalSpace, verticalSpace), _colorContentTable, teamData.Data[i].Role, _smallFont);
            GUIDrawRect(new Rect(3 * horizontalSpace, verticalSpace * (i + 3), horizontalSpace, verticalSpace), _colorContentTable, teamData.Data[i].Nickname, _smallFont);
        }
    }

    /// <summary>
    /// Draws a rectangle
    /// </summary>
    /// <param name="position">Rectangle position</param>
    /// <param name="color">Color to be filled with</param>
    /// <param name="text">Text to be shown inside the rectangle</param>
    /// <param name="fontStyle">Font style to be used in text</param>
    public void GUIDrawRect(Rect position, Color color, string text, GUIStyle fontStyle)
    {
        _defaultTexture.SetPixel(0, 0, color);
        _defaultTexture.Apply();
        _defaultStyle.normal.background = _defaultTexture;
        GUI.Box(position, string.Empty, _defaultStyle);
        GUI.Label(position, text, fontStyle);
    }

    /// <summary>
    /// It only retrieves data when File changes. Uses Checksum validation to check if the file is different from last reading
    /// </summary>
    /// <returns>TeamMembersTable object</returns>
    private TeamMembersTable RetrieveData()
    {
        try
        {
            var assetPath = Path.Combine(Application.streamingAssetsPath, "JsonChallenge.json");
            if (!isReadingFile)
            {
                isReadingFile = true;
                var newChecksum = ComputeHash(assetPath);
                if (newChecksum != latestMd5)
                {
                    UnityEngine.Debug.Log("Change detected in file");
                    var jsonMembers = File.ReadAllText(assetPath);
                    jsonMembers = Regex.Replace(jsonMembers, @"\,(?=\s*?[\}\]])", ""); //remove trailing commas in json (the example provided had this error)
                    teamsMemberTable = JsonUtility.FromJson<TeamMembersTable>(jsonMembers);
                    latestMd5 = newChecksum;
                }
                isReadingFile = false;
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("There was a problem reading the file, maybe it was in use in another process: " + ex.Message);
            isReadingFile = false;
        }
        return teamsMemberTable;
    }

    /// <summary>
    /// Compute the current hash from a file in disk
    /// </summary>
    /// <param name="filePath">the path to the file</param>
    /// <returns>a string representing the checksum</returns>
    string ComputeHash(string filePath)
    {
        var resultingHash = string.Empty;
        using (var md5 = MD5.Create())
        {
            resultingHash = Encoding.Default.GetString(md5.ComputeHash(File.ReadAllBytes(filePath)));
        }
        return resultingHash;
    }

    //Models
    [Serializable]
    public class TeamMembersTable
    {
        public string Title;

        public List<string> ColumnHeaders;

        public List<TeamMember> Data;
    }

    [Serializable]
    public class TeamMember
    {
        public string ID;
        public string Name;
        public string Role;
        public string Nickname;
    }
}