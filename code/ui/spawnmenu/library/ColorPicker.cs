using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

[UseTemplate]
public class ColorPicker : Panel
{
	public Action<Color> OnValueChanged;
	public Action<Color> OnFinalValue;

	protected Panel ColorPreview { get; set; }
	protected Panel PickerCursor { get; set; }
	protected Panel PickerPanel { get; set; }
	protected Panel SliderPanel { get; set; }
	protected Slider TransSlider;
	protected Slider HueSlider;

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
		AddClass( "open" );

		HueSlider = new( 0, 360, true );
		SliderPanel.AddChild( HueSlider );
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

		TransSlider = new( 0, 1, true );
		SliderPanel.AddChild( TransSlider );
		TransSlider.AddClass( "trans" );

		TransSlider.OnValueChanged = ( float trans ) =>
		{
			ColorHSV = ColorHSV.WithAlpha( trans );
			OnValueChanged?.Invoke( ColorHSV );
		};

		TransSlider.OnFinalValue = ( float trans ) =>
		{
			ColorHSV = ColorHSV.WithAlpha( trans );
			OnFinalValue?.Invoke( ColorHSV );
		};

		ColorHSV = Color.Blue;
	}

	public void UpdateUI()
	{
		PickerPanel.Style.BackgroundColor = ColorHSV.WithSaturation( 1 ).WithValue( 1 ).WithAlpha(1);
		ColorPreview.Style.BackgroundColor = ColorHSV;

		PickerCursor.Style.Left = Length.Percent( ColorHSV.Saturation * 100 );
		PickerCursor.Style.Top = Length.Percent( (1 - ColorHSV.Value) * 100 );
		PickerCursor.Style.BackgroundColor = ColorHSV.WithAlpha( 1 );

		TransSlider.Value = ColorHSV.Alpha;
		HueSlider.Value = ColorHSV.Hue;

		string hexColor = ColorHSV.WithAlpha( 1 ).ToColor().Hex;
		TransSlider.Style.Set( "background", $"linear-gradient(to bottom, {hexColor} 0%, rgba({hexColor}, 0) 100%)" );
	}

	bool IsClicking = false;
	public void PickerClicking( bool isClicking )
	{
		IsClicking = isClicking;

		if ( !isClicking ) OnFinalValue?.Invoke(ColorHSV); // Call value changed callback
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
