using Godot;
using System;

public class Game : Control
{

    private void _on_Button_button_down()
    {
        GetTree().ChangeScene("res://Source/Menu.tscn");
    }
}
