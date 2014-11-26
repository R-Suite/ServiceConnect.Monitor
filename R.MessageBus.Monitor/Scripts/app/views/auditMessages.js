define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/auditMessages.html',
    "app/collections/auditMessages",
    "app/views/auditTable",
    "app/views/auditHistogram",
    "app/helpers/timer"
], function(Backbone, _, $, template, AuditMessagesCollection, AuditTableView, AuditHistogramView, Timer) {

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
            this.from = moment.utc().subtract(5, "minutes").format();
            this.to = moment.utc().format();
            this.timer = new Timer(this._updateCollection, 5000);
        },

        render: function() {
            this.$el.html(template);

            this.$el.find(".dateRange").hide();

            this.$el.find('.date').datetimepicker({
                autoclose: true,
                format: "DD/MM/YYYY HH:mm:ss"
            });

            this.$el.find('.from').val(moment.utc(this.from).format("DD/MM/YYYY HH:mm:ss"));
            this.$el.find('.to').val(moment.utc(this.to).format("DD/MM/YYYY HH:mm:ss"));

            this.collection = new AuditMessagesCollection();
            this.timer.start();

            this.collection.fetch({
                success: this._renderViews,
                data: {
                    from: this.from,
                    to: this.to,
                    tags: this.tags
                }
            });

            var that = this;
            this.$el.find(".tags").select2({
                multiple: true,
                allowClear: true,
                query: function(query) {
                    $.ajax({
                        url: "/tags",
                        data: {
                            query: query.term
                        },
                        success: function(data) {
                            var results = [];
                            for (var i = 0; i < data.length; i++) {
                                results.push({
                                    id: data[i],
                                    text: data[i]
                                });
                            }
                            query.callback({
                                results: results
                            });
                        }
                    });
                }
            }).on("select2-selecting", function(e, d) {
                that.tags.push(e.object.id);
                that._filterAuditMessages();
            }).on("select2-removed", function(e, d) {
                var index;
                $.each(that.tags, function(i, tag) {
                    if (tag === e.val) {
                        index = i;
                    }
                });
                that.tags.splice(index, 1);
                that._filterAuditMessages();
            });

            return this;
        },

        _filterAuditMessages: function() {
            var that = this;
            this.collection.fetch({
                data: {
                    from: this.from,
                    to: this.to,
                    tags: this.tags
                },
                reset: true,
                success: function() {
                    that.auditHistogramView.refresh();
                }
            });
        },

        _renderViews: function() {
            this.auditHistogramView = new AuditHistogramView({
                collection: this.collection.fullCollection
            });
            this.$el.find(".histogram").html(this.auditHistogramView.$el);
            this.renderView(this.auditHistogramView);
            this.auditTableView = new AuditTableView({
                collection: this.collection
            });
            this.$el.find(".auditTable").html(this.auditTableView.$el);
            this.renderView(this.auditTableView);

            Backbone.Hubs.AuditHub.on("audits", this._addAudits);
        },

        _setRange: function() {
            Backbone.Hubs.AuditHub.off("audits", this._addAudits);
            this.$el.find(".dateRange").hide();

            var from = this._getFromTime();
            var to = moment.utc();
            this.from = from.format();
            this.to = to.format();

            var range = this.$el.find(".timeRange").val();
            if (range !== "Custom Range") {
                Backbone.Hubs.AuditHub.on("audits", this._addAudits);
            } else {
                this.$el.find(".dateRange").show();
            }

            this.$el.find('.from').val(from.format("DD/MM/YYYY HH:mm:ss"));
            this.$el.find('.to').val(to.format("DD/MM/YYYY HH:mm:ss"));

            this.collection.fetch({
                data: {
                    from: this.from,
                    to: this.to,
                    tags: this.tags
                },
                reset: true
            });
        },

        _addAudits: function(audits) {
            for (var i = 0; i < audits.length; i++) {
                this.collection.fullCollection.add(new Backbone.Model(audits[i]), {
                    at: 0
                });
            }
            this.auditHistogramView.addAudits(audits);
        },

        _updateCollection: function() {
            var from = this._getFromTime();

            for (var i = this.collection.fullCollection.length - 1; i >= 0; i--) {
                var model = this.collection.fullCollection.at(i);
                if (moment.utc(model.get("TimeSent")) < from) {
                    this.collection.fullCollection.remove(model);
                } else {
                    break;
                }
            }
        },

        _getFromTime: function() {
            var from;
            var range = this.$el.find(".timeRange").val();
            switch (range) {
                case "Last 5m":
                    from = moment.utc().subtract(5, "minutes");
                    break;
                case "Last 15m":
                    from = moment.utc().subtract(15, "minutes");
                    break;
                case "Last 30m":
                    from = moment.utc().subtract(30, "minutes");
                    break;
                case "Last 1h":
                    from = moment.utc().subtract(60, "minutes");
                    break;
                case "Last 2h":
                    from = moment.utc().subtract(120, "minutes");
                    break;
                case "Last 4h":
                    from = moment.utc().subtract(240, "minutes");
                    break;
                case "Last 6h":
                    from = moment.utc().subtract(360, "minutes");
                    break;
                case "Last 12h":
                    from = moment.utc().subtract(720, "minutes");
                    break;
                case "Last 24h":
                    from = moment.utc().subtract(1440, "minutes");
                    break;
                case "Last 2d":
                    from = moment.utc().subtract(2880, "minutes");
                    break;
                case "Last 7d":
                    from = moment.utc().subtract(10080, "minutes");
                    break;
                case "Last 14d":
                    from = moment.utc().subtract(20160, "minutes");
                    break;
                case "Last 30d":
                    from = moment.utc().subtract(43200, "minutes");
                    break;
                case "Custom Range":
                    from = moment.utc(this.$el.find(".from").val(), "DD/MM/YYYY HH:mm:ss");
                    break;
                default:
                    from = moment.utc().subtract(5, "minutes");
            }
            return from;
        },

        onClose: function() {
            Backbone.Hubs.AuditHub.off("audits", this._addAudits);
            this.timer.stop();
        }
    });

    return view;
});
