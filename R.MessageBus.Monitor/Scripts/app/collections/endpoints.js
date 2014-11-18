define([
    'backbone',
    'backbone-pageable'
], function(Backbone) {

    var collection = Backbone.PageableCollection.extend({
        url: "endpoints",
        mode: "client"
    });

    return collection;
});
