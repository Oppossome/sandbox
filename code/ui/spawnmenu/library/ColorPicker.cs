using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
public class ColorPicker : Panel
{
	public Action<Color> OnValueChanged;
	public Action<Color> OnFinalValue;
	protected Slider TransSlider;
	protected Slider HueSlider;
	protected bool IsOpen;

	protected Panel ColorPreview { get; set; }
	protected Panel PickerCursor { get; set; }
	protected Panel PickerPanel { get; set; }
	protected Panel SliderPanel { get; set; }
	public string TabText { get; set; } = "⯇";

	private ColorHsv colorHSV = new ColorHsv( 255, 1, 1 );
	public ColorHsv ColorHSV
	{
		get => colorHSV;
		set
		{
			colorHSV = value;
			UpdateUI();
		}
	}

	public ColorPicker()
	{
		PickerCursor.BindClass("active", () => IsClicking );
		BindClass( "open", () => IsOpen );

		HueSlider = SliderPanel.Add.Slider( 0, 360, true );
		HueSlider.AddClass( "hue" );

		HueSlider.OnValueChanged = ( float hue ) =>
		{
			ColorHSV = ColorHSV.WithHue( hue );
			OnValueChanged?.Invoke( ColorHSV );
		};

		HueSlider.OnFinalValue = ( float hue ) =>
		{
			ColorHSV = ColorHSV.WithHue( hue );
			OnFinalValue?.Invoke( ColorHSV );
		};

		TransSlider = SliderPanel.Add.Slider( 0, 1, true );
		TransSlider.AddClass( "trans" );

		TransSlider.OnValueChanged = ( float trans ) =>
		{
			ColorHSV = ColorHSV.WithAlpha( 1 - trans );
			OnValueChanged?.Invoke( ColorHSV );
		};

		TransSlider.OnFinalValue = ( float trans ) =>
		{
			ColorHSV = ColorHSV.WithAlpha( 1 - trans );
			OnFinalValue?.Invoke( ColorHSV );
		};

		OnValueChanged += ( Color clr ) => CreateEvent( "onValueChanged" );
		OnFinalValue += ( Color clr ) => CreateEvent( "onFinalValue" );
		ColorHSV = Color.White;
	}

	public void ToggleWindow()
	{
		IsOpen = !IsOpen;
		TabText = IsOpen ? "⯆" : "⯇";
	}

	public void UpdateUI()
	{
		PickerPanel.Style.BackgroundColor = ColorHSV.WithSaturation( 1 ).WithValue( 1 ).WithAlpha(1);
		ColorPreview.Style.BackgroundColor = ColorHSV;

		PickerCursor.Style.Left = Length.Percent( ColorHSV.Saturation * 100 );
		PickerCursor.Style.Top = Length.Percent( (1 - ColorHSV.Value) * 100 );
		PickerCursor.Style.BackgroundColor = ColorHSV.WithAlpha( 1 );

		TransSlider.Value = 1 - ColorHSV.Alpha;
		HueSlider.Value = ColorHSV.Hue;

		string hexColor = ColorHSV.WithAlpha( 1 ).ToColor().Hex;
		TransSlider.Style.Set( "background", $"linear-gradient(to top, {hexColor} 0%, rgba({hexColor}, 0) 100%)" );
	}

	bool IsClicking = false;
	public void PickerClicking( bool isClicking )
	{
		IsClicking = isClicking;

		if ( !isClicking ) OnFinalValue?.Invoke(ColorHSV);
		else PickerMove();
	}
	public void PickerMove()
	{
		if ( !IsClicking ) return;

		Vector2 pickerBounds = new( PickerPanel.Box.Right - PickerPanel.Box.Left, PickerPanel.Box.Bottom - PickerPanel.Box.Top );
		Vector2 localPos = PickerPanel.ScreenPositionToPanelPosition( Mouse.Position ) / pickerBounds;

		ColorHSV = ColorHSV.WithSaturation( Math.Clamp( localPos.x, 0, 1 ) )
			.WithValue( Math.Clamp( 1 - localPos.y, 0, 1 ) );

	}
}

namespace Sandbox.UI.Construct
{
	public static class ColorPickerCreator
	{
		public static ColorPicker ColorPicker( this PanelCreator self, Action<Color> callback )
		{
			ColorPicker newColorPicker = new();
			newColorPicker.OnFinalValue = callback;
			self.panel.AddChild( newColorPicker );
			return newColorPicker;
		}
	}
}
