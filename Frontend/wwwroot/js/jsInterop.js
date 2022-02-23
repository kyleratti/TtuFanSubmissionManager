window.getWindowDimensions = () => ({
    width: window.innerWidth,
    height: window.innerHeight
});

window.getImageDimensions = (elementId) =>
{
    const element = document.getElementById(elementId);

    if (element === null)
        return null;

    return {
        width: element.naturalWidth,
        height: element.naturalHeight
    };
}

window.isImageOrientationSupported = () => CSS.supports("image-orientation", "from-image");

Blazor.registerCustomEventType("fullscreenmodechange", {
    browserEventName: "fullscreenchange",
    createEventArgs: _ => {
        console.log("creating event args");
        return {
            isInFullScreen: document.fullscreenElement === null
        }
    }
});