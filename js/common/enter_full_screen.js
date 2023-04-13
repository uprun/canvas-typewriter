if (typeof(common) == "undefined")
{
    common = {};
}
common.enter_full_screen = async function()
{
    var element = document.documentElement;
    const request_full_screen = element.requestFullscreen || element.webkitRequestFullScreen || element.mozRequestFullScreen || element.msRequestFullscreen;
    try
    {
        await request_full_screen.call(element);
    }
    catch(e)
    {
        console.log(e);
    }
    
    console.log("entering full screen");
};