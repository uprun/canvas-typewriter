AST.ko__function_body_on_key_down = function(item)
{
    console.log("key down")
    console.log(item)
    var target = event.originalTarget || event.target;
    if (event.code == "Enter")
    {
        event.stopPropagation();
        var to_add = { type: "lisperanto_atom", text: target.textContent};
        item.list_of_expressions.push(to_add);
        item.new_expression("");
        item.new_expression("new expression");
        return false;
    }
    return true;

};