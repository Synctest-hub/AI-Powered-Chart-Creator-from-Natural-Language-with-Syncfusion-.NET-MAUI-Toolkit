using Syncfusion.Maui.AIAssistView;
using Syncfusion.Maui.Toolkit.Buttons;

namespace ChartGenerator
{
    public class ChartViewBehavior : Behavior<ContentPage>
    {
        /// <summary>
        /// Holds the ai assistview instance.
        /// </summary>
        private SfAIAssistView? sfAIAssistView;

        /// <summary>
        /// Holds the button instance.
        /// </summary>
        private SfButton? aiButton;

        /// <summary>
        /// Animation for AI button.
        /// </summary>
        private Animation? animation;

        /// <summary>
        /// Holds the header view.
        /// </summary>
        private Border? headerView { get; set; }

        /// <summary>
        /// The scheduler behavior.
        /// </summary>
        public ChartViewBehavior()
        {
            animation = new Animation();
        }

        /// <summary>
        /// Begins when the behavior attached to the view.
        /// </summary>
        /// <param name="bindable">The bindable element.</param>
        protected override void OnAttachedTo(ContentPage bindable)
        {
            base.OnAttachedTo(bindable);
            this.sfAIAssistView = bindable.Content.FindByName<SfAIAssistView>("aiAssistView");
            this.aiButton = bindable.Content.FindByName<SfButton>("aibutton");
            this.headerView = bindable.Content.FindByName<Border>("headerView");
            this.aiButton.Clicked += OnClickToShowAssistView!;
            this.StartAnimation();
            InitialAppointmentBooking();
        }

        /// <summary>
        /// Method to start animation for ai button.
        /// </summary>
        private async void StartAnimation()
        {
            if (this.aiButton != null && this.animation != null)
            {
                var bubbleEffect = new Animation(v => this.aiButton.Scale = v, 1, 1.15, Easing.CubicInOut);
                var fadeEffect = new Animation(v => this.aiButton.Opacity = v, 1, 0.5, Easing.CubicInOut);

                animation.Add(0, 0.5, bubbleEffect);
                animation.Add(0, 0.5, fadeEffect);
                animation.Add(0.5, 1, new Animation(v => this.aiButton.Scale = v, 1.15, 1, Easing.CubicInOut));
                animation.Add(0.5, 1, new Animation(v => this.aiButton.Opacity = v, 0.5, 1, Easing.CubicInOut));
                await Task.Delay(250);

                animation.Commit(this.aiButton, "BubbleEffect", length: 1500, easing: Easing.CubicInOut, repeat: () => true);
            }
        }

        /// <summary>
        /// Method to stop animation for ai button.
        /// </summary>
        private void StopAnimation()
        {
            if (this.aiButton != null)
            {
                this.aiButton.AbortAnimation("BubbleEffect");
                this.aiButton.Scale = 1;
                this.aiButton.Opacity = 1;
            }
        }

        /// <summary>
        /// Method to get the initial appointments.
        /// </summary>
        /// <returns></returns>
        private void InitialAppointmentBooking()
        {

        }

        /// <summary>
        /// Method to open the popup view.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args</param>
        private void OnClickToShowAssistView(object sender, EventArgs e)
        {
            this.StopAnimation();
            if (sfAIAssistView != null)
            {
                bool isVisible = !sfAIAssistView.IsVisible;
                sfAIAssistView.IsVisible = isVisible;
                this.headerView!.IsVisible = isVisible;
            }
        }

        /// <summary>
        /// On Detached method.
        /// </summary>
        /// <param name="bindable">The bindable element.</param>
        protected override void OnDetachingFrom(ContentPage bindable)
        {
            base.OnDetachingFrom(bindable);

            if (this.aiButton != null)
            {
                this.aiButton.Clicked -= OnClickToShowAssistView!;
                if (this.aiButton.AnimationIsRunning("BubbleEffect"))
                {
                    this.aiButton.AbortAnimation("BubbleEffect");
                }
            }

            if (this.animation != null)
            {
                this.animation.Dispose();
            }

            this.animation = null;
        }
    }
}
