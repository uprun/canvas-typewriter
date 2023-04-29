AST.ko__function_lisperanto_atom_on_key_down = function(item)
{
    console.log("lisperanto atom key down")
    console.log(event)
    console.log(item);
    var target = event.originalTarget || event.target;
    if ( (event.metaKey || event.ctrlKey) && event.code == "Period")
    {
        event.stopPropagation();
        console.log("Show transform invocation");
        return false;
    }
    if (event.code == "Enter")
    {
        event.stopPropagation();
        return false;
    }
    return true;
};