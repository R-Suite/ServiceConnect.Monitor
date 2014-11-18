define([
    'backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/endpoints.html',
    "app/collections/services",
    "app/collections/serviceMessages",
    "app/collections/endpoints",
    "app/views/services",
    "app/views/endpointGraph"
], function(Backbone, _, $, template, ServiceCollection, ServiceMessagesCollection, EndpointCollection, ServicesView, EndpointGraphView) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(template);

            this.serviceMessagesCollection = new ServiceMessagesCollection();
            this.endpointCollection = new EndpointCollection();
            var promise1 = this.endpointCollection.fetch({
                success: this._renderServices
            });
            var promise2 = this.serviceMessagesCollection.fetch();
            $.when(promise1, promise2).done(this._renderEndpointGraph);
            return this;
        },

        _renderServices: function() {
            var services = new ServicesView({
                collection: this.endpointCollection
            });
            this.renderView(services);
            this.$el.find(".services").html(services.$el);
        },

        _renderEndpointGraph: function() {
            var endpointGraph = new EndpointGraphView({
                endpointCollection: this.endpointCollection.fullCollection,
                serviceMessagesCollection: this.serviceMessagesCollection
            });
            this.$el.find(".endpointGraph").html(endpointGraph.$el);
            this.renderView(endpointGraph);
        }
    });

    return view;
});
