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
	public Slider Slider;

	public float Value
	{
		get => MathF.Floor( Slider.Value / SliderStep ) * SliderStep;
		set {
			Slider.Value = value;
			UpdateUI();
		}
	}

	public SliderLabeled(string title, float min, float max, float step)
	{
		Slider = SliderSpot.Add.Slider( min, max );
		SliderStep = step;
		TextName = title;

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

		Value = min;
		ValueEntry.Text = min.ToString();
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
