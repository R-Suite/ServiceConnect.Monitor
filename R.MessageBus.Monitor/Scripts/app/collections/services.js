define(['backbone','backbone-pageable'], function(Backbone) {

    "use strict";
    
    var collection = Backbone.PageableCollection.extend({
        url: "services",
        mode: "client"
    });

    return collection;
});
