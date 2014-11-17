define([
    'backbone'
], function(Backbone) {

    var collection = Backbone.Collection.extend({
        url: "serviceMessages"
    });

    return collection;
});
