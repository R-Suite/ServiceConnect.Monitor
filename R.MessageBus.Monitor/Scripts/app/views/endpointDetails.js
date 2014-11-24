define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/endpointDetails.html',
    'app/views/serviceTable',
    'app/views/serviceGraph',
    'app/views/serviceDetails',
    'select2'
], function(Backbone, _, $, template, ServiceTableView, ServiceGraphView, ServiceDetailsView) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        events: {
            "change .from": "_fetchHeartbeats",
            "change .to": "_fetchHeartbeats"
        },

        initialize: function() {
            _.bindAll(this);
            this.tags = [];
        },

        render: function() {
            this.$el.html(_.template(template, {
                header: this.model.get("Name") + " (" + this.model.get("InstanceLocation") + ")"
            }));

            this.$el.find('.date').datetimepicker({
                autoclose: true,
                format: "DD/MM/YYYY HH:mm:ss"
            });
            this.$el.find('.from').val(moment.utc(this.model.get("From")).format("DD/MM/YYYY HH:mm:ss"));
            this.$el.find('.to').val(moment.utc(this.model.get("To")).format("DD/MM/YYYY HH:mm:ss"));

            this.serviceTable = new ServiceTableView({
                collection: this.collection,
                model: this.model
            });
            this.renderView(this.serviceTable);
            this.$el.find(".serviceTable").html(this.serviceTable.$el);
            this.serviceGraph = new ServiceGraphView({
                collection: this.collection.fullCollection,
                model: this.model
            });
            this.renderView(this.serviceGraph);
            this.$el.find(".serviceGraph").html(this.serviceGraph.$el);
            this.details = new ServiceDetailsView({
                model: this.model
            });
            this.renderView(this.details);
            this.$el.find(".serviceDetails").html(this.details.$el);
            
            return this;
        },

        _fetchHeartbeats: function() {
            this.model.set("From", moment.utc(this.$el.find(".from").val(), "DD/MM/YYYY HH:mm:ss").format());
            this.model.set("To", moment.utc(this.$el.find(".to").val(), "DD/MM/YYYY HH:mm:ss").format());
            this.collection.fetch({
                data: {
                    name: this.model.get("Name"),
                    location: this.model.get("InstanceLocation"),
                    from: this.model.get("From"),
                    to: this.model.get("From")
                },
                reset: true
            });
        }
    });

    return view;
});
