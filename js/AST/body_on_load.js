if (typeof(AST) == "undefined")
{
    AST = {};
}
AST.body_on_load = function()
{
    var body = document.getElementById("body");
    ko.applyBindings(AST);
    
};