define(['backbone', 'backbone-pageable'], function(Backbone) {

    "use strict";

    var collection = Backbone.PageableCollection.extend({
        mode: "client",
        state: {
            pageSize: 5
        }
    });

    return collection;
});
