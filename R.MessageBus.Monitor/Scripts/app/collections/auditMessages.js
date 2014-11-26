define(['backbone', 'backbone-pageable'], function(Backbone) {

    "use strict";

    var collection = Backbone.PageableCollection.extend({
        url: "audits",
        mode: "client"
    });

    return collection;
});
