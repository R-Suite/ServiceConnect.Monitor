define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/endpointDetails.html'
], function(Backbone, _, $, template) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);
            return this;
        }
    });

    return view;
});
