// Native iOS haptics for Ricomet. Called from IOSHaptics.cs.
#import <UIKit/UIKit.h>

extern "C" void _RicometHapticImpact(int style)
{
    UIImpactFeedbackStyle feedbackStyle = UIImpactFeedbackStyleLight;
    if (style == 1) feedbackStyle = UIImpactFeedbackStyleMedium;
    else if (style >= 2) feedbackStyle = UIImpactFeedbackStyleHeavy;

    UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:feedbackStyle];
    [generator impactOccurred];
}
