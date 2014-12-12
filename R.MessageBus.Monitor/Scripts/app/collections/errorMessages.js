define(['backbone', 'backbone-pageable'], function(Backbone) {

    "use strict";

    var collection = Backbone.PageableCollection.extend({
        url: "errors",
        mode: "client"
    });

    return collection;
});
