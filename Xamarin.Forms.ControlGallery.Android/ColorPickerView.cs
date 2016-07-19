using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Droid = Android;

namespace Xamarin.Forms.ControlGallery.Android
{

	public class ColorPickerView : ImageView, INotifyPropertyChanged, Droid.Views.View.IOnTouchListener
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
			colorPointerPaint = new Droid.Graphics.Paint();
			colorPointerPaint.SetStyle(Droid.Graphics.Paint.Style.Stroke);
			colorPointerPaint.StrokeWidth = 5f;
			colorPointerPaint.SetARGB(255, 0, 0, 0);

			colorPreviewPaint = new Droid.Graphics.Paint();
			colorPreviewPaint.SetStyle(Droid.Graphics.Paint.Style.Fill);
			colorPreviewPaint.SetARGB(0, 0, 0, 0);
			DrawingCacheEnabled = true;
			this.SetOnTouchListener(this);
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
				OnPropertyChanged();
				OnColorPicked();
			}
		}

		protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
		{

			var _paint = new Droid.Graphics.Paint();

			using (Droid.Graphics.Shader s = new Droid.Graphics.LinearGradient(0, 0, Width, Height, COLORS, null, Droid.Graphics.Shader.TileMode.Mirror))
			{
				_paint = new Droid.Graphics.Paint(Droid.Graphics.PaintFlags.AntiAlias);
				_paint.SetShader(s);
				_paint.SetStyle(Droid.Graphics.Paint.Style.FillAndStroke);
			}
			canvas.DrawPaint(_paint);

			BuildDrawingCache();

			if (currentTouch != null)
			{
				colorPreviewPaint.Color = previewColor;

				canvas.DrawOval(new Droid.Graphics.RectF(currentTouch.X - ovalWidth, currentTouch.Y - ovalWidth, currentTouch.X + ovalWidth, currentTouch.Y + ovalWidth), colorPointerPaint);

				canvas.DrawOval(new Droid.Graphics.RectF(currentTouch.X - ovalColorPreviewWidth, currentTouch.Y - ovalColorPreviewWidth, currentTouch.X + ovalColorPreviewWidth, currentTouch.Y + ovalColorPreviewWidth), colorPreviewPaint);

			}
			_paint.Dispose();
			base.DispatchDraw(canvas);
		}

		bool IOnTouchListener.OnTouch(Droid.Views.View v, MotionEvent e)
		{
			System.Diagnostics.Debug.WriteLine(e.Action);
			if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
			{
				GetCurrentColor((int)e.GetX(), (int)e.GetY());
			}
			if (e.Action == MotionEventActions.Up)
			{
				SelectedColor = previewColor;
			}
			return true;
		}

		float ovalWidth = 20;
		float ovalColorPreviewWidth = 10;
		Droid.Graphics.Paint colorPointerPaint;
		Droid.Graphics.Paint colorPreviewPaint;
		Droid.Graphics.Color selectedColor;
		Droid.Graphics.Color previewColor { get; set; }
		Droid.Graphics.PointF currentTouch;

		void GetCurrentColor(int x, int y)
		{
			currentTouch = new Droid.Graphics.PointF(x, y);

			Droid.Graphics.Bitmap bmp = GetDrawingCache(true);
			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (x >= bmp.Width)
				x = bmp.Width - 1;
			if (y >= bmp.Height)
				y = bmp.Height - 1;

			int color = bmp.GetPixel(x, y);
			previewColor = new Droid.Graphics.Color(color);

			Invalidate();
		}

		void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		void OnColorPicked()
		{
			var handler = ColorPicked;
			if (handler != null)
				handler(this, new EventArgs());
		}
	}
}

