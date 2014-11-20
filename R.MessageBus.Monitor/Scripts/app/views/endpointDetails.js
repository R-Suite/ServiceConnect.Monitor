define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/endpointDetails.html',
    'app/views/serviceTable',
    'app/views/serviceGraph'
], function(Backbone, _, $, template, ServiceTableView, ServiceGraphView) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        events: {
            "change .from": "_fetchHeartbeats",
            "change .to": "_fetchHeartbeats"
        },

        initialize: function() {
            _.bindAll(this);
        },

        render: function() {
            this.$el.html(_.template(template, {
                header: this.model.get("Name") + " (" + this.model.get("Location") + ")"
            }));

            this.$el.find('.date').datetimepicker({
                autoclose: true,
                format: "DD/MM/YYYY HH:mm:ss"
            });
            this.$el.find('.from').val(moment.utc(this.model.get("From")).format("DD/MM/YYYY HH:mm:ss"));
            this.$el.find('.to').val(moment.utc(this.model.get("To")).format("DD/MM/YYYY HH:mm:ss"));

            var serviceTable = new ServiceTableView({
                collection: this.collection
            });
            this.renderView(serviceTable);
            this.$el.find(".serviceTable").html(serviceTable.$el);
            var serviceGraph = new ServiceGraphView({
                collection: this.collection.fullCollection
            });
            this.renderView(serviceGraph);
            this.$el.find(".serviceGraph").html(serviceGraph.$el);
            return this;
        },

        _fetchHeartbeats: function() {
            this.model.set("From", moment.utc(this.$el.find(".from").val(), "DD/MM/YYYY HH:mm:ss").format());
            this.model.set("To", moment.utc(this.$el.find(".to").val(), "DD/MM/YYYY HH:mm:ss").format());
            this.collection.fetch({
                data: this.model.attributes,
                reset: true
            });
        }
    });

    return view;
});
