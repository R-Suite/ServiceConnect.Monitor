define(['backbone','backbone-pageable'], function(Backbone) {

    "use strict";
    
    var collection = Backbone.PageableCollection.extend({
        url: "endpoints",
        mode: "client"
    });

    return collection;
});
