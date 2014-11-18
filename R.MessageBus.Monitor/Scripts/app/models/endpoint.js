define(['backbone'], function(Backbone) {

    "use strict";
    
    var model = Backbone.Model.extend({
        urlRoot: "endpoint"
    });

    return model;
});
