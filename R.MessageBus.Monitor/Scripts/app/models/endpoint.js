define([
    'backbone'
], function(Backbone) {

    var model = Backbone.Model.extend({
        urlRoot: "endpoint"
    });

    return model;
});
