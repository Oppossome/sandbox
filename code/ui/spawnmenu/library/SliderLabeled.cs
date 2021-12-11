using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
public class SliderLabeled : Panel
{
	public Action<float> OnValueChanged;
	public Action<float> OnFinalValue;
	float SliderStep;

	public TextEntry ValueEntry { get; set; }
	public Panel SliderSpot { get; set; }
	public string TextName { get; set; }
	public Slider Slider = new();

	public float Value
	{
		get => MathF.Floor( Slider.Value / SliderStep ) * SliderStep;
		set {
			Slider.Value = value;
			UpdateUI();
		}
	}

	public SliderLabeled()
	{
		Slider = AddChild<Slider>();

		Slider.OnValueChanged = ( float val ) =>
		{
			ValueEntry.Text = Value.ToString();
			OnValueChanged?.Invoke( Value );
		};

		Slider.OnFinalValue = ( float val ) =>
		{
			ValueEntry.Text = Value.ToString();
			OnFinalValue?.Invoke( Value );
		};

		ValueEntry.BindClass( "active", () => ValueEntry.HasActive );
		ValueEntry.Numeric = true;

		ValueEntry.AddEventListener( "onchange", () =>
		{
			if ( float.TryParse( ValueEntry.Text, out float val ) )
			{
				Value = val;
				OnFinalValue?.Invoke( Value );
			}
		} );

		OnFinalValue += ( float val ) => CreateEvent( "onFinalValue" );
		OnValueChanged += ( float val ) => CreateEvent( "onValueChanged" );
	}

	public SliderLabeled(string title, float min, float max, float step) : this()
	{
		SliderStep = step;
		TextName = title;
		Slider.Min = min;
		Slider.Max = max;

		ValueEntry.Text = min.ToString();		
		Value = min;
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name )
		{
			case "step":
				SliderStep = float.Parse( value );
				ValueEntry.Text = Slider.Min.ToString();
				Value = Slider.Min;
				return;
			case "title":
				TextName = value;
				return;
		}

		Slider.SetProperty( name, value );
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
	}

	protected void UpdateUI() =>
		ValueEntry.Text = Value.ToString();
	
}

namespace Sandbox.UI.Construct
{
	public static class LabeledSliderCreator
	{
		public static SliderLabeled SliderLabeled( this PanelCreator self, string title, float min, float max, float step = .1f )
		{
			SliderLabeled nSlider = new(title, min, max, step);
			self.panel.AddChild( nSlider );
			return nSlider;
		}
	}
}
