using Godot;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class LoginScreen : Control
{
    LineEdit LoginLine = null!;
    LineEdit PasswordLine = null!;
    HTTPRequest request = null!;
    Label failLabel = null!;
 
    public override void _EnterTree()
    {
        LoginLine = (LineEdit)GetNode("LoginLine");
        PasswordLine = (LineEdit)GetNode("PasswordLine");
        request = (HTTPRequest)GetNode("HTTPRequest");
        failLabel = (Label)GetNode("PasswordLine/Fail info");
    }

    private void _on_LoginButton_button_down()
    {
        string query = JsonConvert.SerializeObject(new LoginBody(_email:LoginLine.Text,_password:PasswordLine.Text));
        GD.Print(query);
        HTTPRequest httpRequest = GetNode<HTTPRequest>("HTTPRequest");
        string[] headers = new string[] { "Content-Type: application/json" };
        httpRequest.Request("http://127.0.0.1:3000/login.json", headers, false, HTTPClient.Method.Post, query);
        
    }

    private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        var str = System.Text.Encoding.Default.GetString(body);
        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        if (json is not null && json.ContainsKey("token"))
        {
            GD.Print(json["token"]);
            Net.token = json["token"];
            GetTree().ChangeScene("res://Source/Menu.tscn");
        }
        else 
        failLabel.Text = "Failed to login";
    }
}

