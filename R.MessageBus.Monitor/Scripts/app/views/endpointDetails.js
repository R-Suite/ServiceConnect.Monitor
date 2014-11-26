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
            "change .to": "_fetchHeartbeats",
            "change .timeRange": "_setRange"
        },

        initialize: function() {
            _.bindAll(this);
            this.tags = [];
            Backbone.Hubs.HeartbeatHub.on("heartbeats", this._addHeartbeats);
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
            this.$el.find(".serviceGraph").html(this.serviceGraph.$el);
            this.renderView(this.serviceGraph);

            this.details = new ServiceDetailsView({
                model: this.model
            });
            this.renderView(this.details);
            this.$el.find(".serviceDetails").html(this.details.$el);

            this.$el.find(".dateRange").hide();

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
                    to: this.model.get("To")
                },
                reset: true
            });
        },

        _setRange: function(e) {
            Backbone.Hubs.HeartbeatHub.off("heartbeats", this._addHeartbeats);
            var range = $(e.currentTarget).val();
            var from = moment.utc(this.$el.find(".from").val(), "DD/MM/YYYY HH:mm:ss").format();
            this.$el.find(".dateRange").hide();
            switch (range) {
                case "Last 5m":
                    from = moment.utc().subtract(5, "minutes").format();
                    break;
                case "Last 15m":
                    from = moment.utc().subtract(15, "minutes").format();
                    break;
                case "Last 30m":
                    from = moment.utc().subtract(30, "minutes").format();
                    break;
                case "Last 1h":
                    from = moment.utc().subtract(60, "minutes").format();
                    break;
                case "Last 2h":
                    from = moment.utc().subtract(120, "minutes").format();
                    break;
                case "Last 4h":
                    from = moment.utc().subtract(240, "minutes").format();
                    break;
                case "Last 6h":
                    from = moment.utc().subtract(360, "minutes").format();
                    break;
                case "Last 12h":
                    from = moment.utc().subtract(720, "minutes").format();
                    break;
                case "Last 24h":
                    from = moment.utc().subtract(1440, "minutes").format();
                    break;
                case "Last 2d":
                    from = moment.utc().subtract(2880, "minutes").format();
                    break;
                case "Last 7d":
                    from = moment.utc().subtract(10080, "minutes").format();
                    break;
                case "Last 14d":
                    from = moment.utc().subtract(20160, "minutes").format();
                    break;
                case "Last 30d":
                    from = moment.utc().subtract(43200, "minutes").format();
                    break;
                case "Custom Range":
                    this.$el.find(".dateRange").show();
                    break;
                default:
            }

            this.model.set("From", from);
            this.model.set("To", moment.utc().format());

            if (range !== "Custom Range") {
                Backbone.Hubs.HeartbeatHub.on("heartbeats", this._addHeartbeats);
            }

            this.$el.find('.from').val(moment.utc(this.model.get("From")).format("DD/MM/YYYY HH:mm:ss"));
            this.$el.find('.to').val(moment.utc(this.model.get("To")).format("DD/MM/YYYY HH:mm:ss"));

            this.collection.fetch({
                data: {
                    name: this.model.get("Name"),
                    location: this.model.get("InstanceLocation"),
                    from: this.model.get("From"),
                    to: this.model.get("To")
                },
                reset: true
            });
        },

        _addHeartbeats: function(heartbeats) {
            for (var i = 0; i < heartbeats.length; i++) {
                this.collection.add(new Backbone.Model(heartbeats[i]), {
                    at: 0
                });
            }
            this.serviceGraph.addHeartbeats(heartbeats);
        },

        onClose: function() {
            Backbone.Hubs.HeartbeatHub.off("heartbeats", this._addHeartbeats);
        }
    });

    return view;
});
