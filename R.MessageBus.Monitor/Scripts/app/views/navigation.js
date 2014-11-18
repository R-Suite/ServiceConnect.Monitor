define(['backbone',
        'underscore',
        'jquery',
        'bower_components/requirejs-text/text!app/templates/navigation.html'
    ], function (Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".navigation",

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            return this;
        },

        setActive: function(route) {
            this.$el.find(".navigationItems").removeClass("active");
            this.$el.find("." + route).addClass("active");
        }
    });

    return view;
});
