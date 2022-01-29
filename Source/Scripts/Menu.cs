using Godot;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
 
public class Menu : Control
{
 
    private void _on_PlayButton_button_down()
    {
        GetTree().ChangeScene("res://Source/Main.tscn");
    }
 
    private void _on_LogoutButton_pressed()
    {
        HTTPRequest httpRequest = GetNode<HTTPRequest>("HTTPRequest");
        string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Token token={Net.token}" };
        var result = httpRequest.Request("http://127.0.0.1:3000/logout.json", headers, false, HTTPClient.Method.Get);
 
    }
 
private void _on_HTTPRequest_request_completed(int result, int response_code, string[] headers, byte[] body)
    {
        var str = System.Text.Encoding.Default.GetString(body);
        GD.Print(str);
        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        if (json is not null && json.ContainsKey("message"))
        {
            GD.Print("Logout");
            Net.token= "";
            GetTree().ChangeScene("res://Source/LoginScreen.tscn");
        }
    }
}
 
