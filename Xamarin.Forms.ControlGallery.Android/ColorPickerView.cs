using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Droid = Android;

namespace Xamarin.Forms.ControlGallery.Android
{
	public class ColorPickerView : ViewGroup, INotifyPropertyChanged
	{
		static readonly int[] COLORS = new[] {
				new Droid.Graphics.Color(255,0,0,255).ToArgb(), new Droid.Graphics.Color(255,0,255,255).ToArgb(), new Droid.Graphics.Color(0,0,255,255).ToArgb(),
				new Droid.Graphics.Color(0,255,255,255).ToArgb(), new Droid.Graphics.Color(0,255,0,255).ToArgb(), new Droid.Graphics.Color(255,255,0,255).ToArgb(),
				new Droid.Graphics.Color(255,0,0,255).ToArgb()
			};

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler ColorPicked;

		public ColorPickerView(Context context, int minWidth, int minHeight) : base(context)
		{
			SelectedColor = Color.Black.ToAndroid();

			SetMinimumHeight(minHeight);
			SetMinimumWidth(minWidth);

			imageViewPallete = new ImageView(context);
			imageViewPallete.DrawingCacheEnabled = true;
			imageViewPallete.Background = new Droid.Graphics.Drawables.GradientDrawable(Droid.Graphics.Drawables.GradientDrawable.Orientation.LeftRight, COLORS);

			imageViewPallete.Touch += (object sender, TouchEventArgs e) =>
			{
				if (e.Event.Action == MotionEventActions.Down || e.Event.Action == MotionEventActions.Move)
				{
					currentPoint = new Droid.Graphics.Point((int)e.Event.GetX(), (int)e.Event.GetY());
					previewColor = GetCurrentColor((int)e.Event.GetX(), (int)e.Event.GetY());
				}
				if (e.Event.Action == MotionEventActions.Up)
				{
					SelectedColor = previewColor;
				}
			};

			imageViewSelectedColor = new ImageView(context);
			colorPointer = new ColorPointer(context);

			AddView(imageViewPallete);
			AddView(imageViewSelectedColor);
			AddView(colorPointer);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			var half = (bottom - top) / 2;
			var margin = 20;

			var palleteY = top + half;

			imageViewSelectedColor.Layout(left, top, right, bottom - half - margin);
			imageViewPallete.Layout(left, palleteY, right, bottom);
			colorPointer.Layout(left, palleteY, right, bottom);
		}

		public Droid.Graphics.Color SelectedColor
		{
			get
			{
				return selectedColor;
			}

			set
			{
				if (selectedColor == value)
					return;

				selectedColor = value;
				UpdateUi();
				OnPropertyChanged();
				OnColorPicked();
			}
		}

		Droid.Graphics.Point currentPoint;
		ColorPointer colorPointer;
		ImageView imageViewSelectedColor;
		ImageView imageViewPallete;
		Droid.Graphics.Color selectedColor;
		Droid.Graphics.Color previewColor { get; set; }
		Droid.Graphics.Bitmap backgroundBitmap;

		void UpdateUi()
		{
			imageViewSelectedColor?.SetBackgroundColor(selectedColor);
			colorPointer?.UpdatePoint(currentPoint);
		}

		Droid.Graphics.Color GetCurrentColor(int x, int y)
		{
			if (backgroundBitmap == null)
				backgroundBitmap = imageViewPallete.GetDrawingCache(false);

			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (x >= backgroundBitmap.Width)
				x = backgroundBitmap.Width - 1;
			if (y >= backgroundBitmap.Height)
				y = backgroundBitmap.Height - 1;

			int color = backgroundBitmap.GetPixel(x, y);
			return new Droid.Graphics.Color(color);
		}

		void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		void OnColorPicked()
		{
			ColorPicked?.Invoke(this, new EventArgs());
		}
	}

	public class ColorPointer : Droid.Views.View
	{
		Droid.Graphics.Paint colorPointerPaint;
		int ovalWidth = 10;
		Droid.Graphics.Point currentPoint;
		Droid.Graphics.Point nextPoint;

		public ColorPointer(Context context) : base(context)
		{

			colorPointerPaint = new Droid.Graphics.Paint();
			colorPointerPaint.SetStyle(Droid.Graphics.Paint.Style.Stroke);
			colorPointerPaint.StrokeWidth = 5f;
			colorPointerPaint.SetARGB(255, 0, 0, 0);
			UpdatePoint(new Droid.Graphics.Point(50, 50));
		}

		public void UpdatePoint(Droid.Graphics.Point p)
		{
			if (p == null)
				return;

			if (currentPoint == null)
				currentPoint = nextPoint;

			nextPoint = p;
			System.Diagnostics.Debug.WriteLine($"Set nextPoint - {p}");

			//left = currentPoint.X - ovalWidth;
			//top = currentPoint.Y - ovalWidth;
			//right = left + ovalWidth;
			//bottom = top + ovalWidth;

			//	Invalidate();
			//	ObjectAnimator.OfObject(marker, "position", evaluator, startLatLng, finalLatLng)
			//.SetDuration(1000)
			//.SetInterpolator(new Android.Views.Animations.BounceInterpolator())
			//.Start();
		}

		int left;
		//	float right;
		int top;
		//	float bottom;

		protected override void OnDraw(Droid.Graphics.Canvas canvas)
		{

			base.OnDraw(canvas);
			canvas.DrawOval(new Droid.Graphics.RectF(left, left + ovalWidth, top, top + ovalWidth), colorPointerPaint);
			//AnimatePoint();

		}

		void AnimatePoint()
		{
			if (nextPoint == null)
				return;

			var finalLeft = nextPoint.X - ovalWidth;
			var finalTop = nextPoint.Y - ovalWidth;
			//	var factor = 20;

			System.Diagnostics.Debug.WriteLine($"final x,y - {finalLeft} : {finalTop}");
			if (left < finalLeft)
			{
				left++;
			}
			else if (left > finalLeft)
			{
				left--;
			}

			if (top < finalTop)
			{
				top++;
			}
			else if (top > finalTop)
			{
				top--;
			}
			Task.Delay(200).Wait();
			System.Diagnostics.Debug.WriteLine($"x,y - {left} : {top}");
			if ((int)left == finalLeft && (int)top == finalTop)
			{
				currentPoint = nextPoint;
				nextPoint = null;
				return;
			}

			Invalidate();
		}
	}
}

