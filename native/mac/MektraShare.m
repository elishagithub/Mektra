#import <Cocoa/Cocoa.h>
static NSSharingServicePicker *mektraPicker;
int MektraShareText(const char *text)
{
    NSString *message=[NSString stringWithUTF8String:text];
    if(!message)return 0;
    dispatch_async(dispatch_get_main_queue(), ^{
        NSWindow *window=NSApp.keyWindow ?: NSApp.mainWindow;
        if(!window)return;
        NSView *view=window.contentView;
        mektraPicker=[[NSSharingServicePicker alloc] initWithItems:@[message]];
        [mektraPicker showRelativeToRect:NSMakeRect(NSMidX(view.bounds),NSMidY(view.bounds),1,1) ofView:view preferredEdge:NSMaxYEdge];
    });
    return 1;
}
