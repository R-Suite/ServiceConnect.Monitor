define([
    'backbone',
    'backbone-pageable'
], function(Backbone) {

    var collection = Backbone.PageableCollection.extend({
        url: "services",
        mode: "client"
    });

    return collection;
});
