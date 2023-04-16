AST.body_on_load = function()
{
    var body = document.getElementById("body");
    ko.applyBindings(AST);
    console.log("body_on_load");
    
};