//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

define(['backbone',
    'underscore',
    'jquery',
    'bower_components/requirejs-text/text!app/templates/errorMessages.html',
    "app/collections/errorMessages",
    "app/views/errorTable",
    "app/views/errorHistogram",
    "app/helpers/timer",
    "toastr",
    "app/views/errorRetryModal"
], function(Backbone, _, $, template, ErrorMessagesCollection, ErrorTableView, ErrorHistogramView, Timer, toastr, RetryModal) {

    "use strict";

    var view = Backbone.View.extend({

        el: ".mainContent",

        events: {
            "change .from": "_fetchErrors",
            "change .to": "_fetchErrors",
            "change .timeRange": "_setRange",
            "click .retryAll": "_retryAllMessages",
            "click .retrySelected": "_retrySelectedMessages"
        },

        initialize: function() {
            _.bindAll(this);
            this.tags = [];
            this.from = moment.utc().subtract(15, "minutes").format();
            this.to = moment.utc().format();
            this.timer = new Timer(this._updateCollection, 2000);
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

            this.collection = new ErrorMessagesCollection();

            this.collection.from = this.from;
            this.collection.to = this.to;
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
                that._filterErrorMessages();
            }).on("select2-removed", function(e, d) {
                var index;
                $.each(that.tags, function(i, tag) {
                    if (tag === e.val) {
                        index = i;
                    }
                });
                that.tags.splice(index, 1);
                that._filterErrorMessages();
            });

            return this;
        },

        _filterErrorMessages: function() {
            var that = this;
            this.collection.from = this.from;
            this.collection.to = this.to;
            this.collection.fetch({
                data: {
                    from: this.from,
                    to: this.to,
                    tags: this.tags.join()
                },
                reset: true,
                success: function() {
                    that.errorTableView.refresh();
                    that.errorHistogramView.refresh();
                }
            });
        },

        _renderViews: function() {
            this.errorHistogramView = new ErrorHistogramView({
                collection: this.collection
            });
            this.$el.find(".errorHistogram").html(this.errorHistogramView.$el);
            this.renderView(this.errorHistogramView);
            this.errorTableView = new ErrorTableView({
                collection: this.collection,
                onRetryComplete: this._filterErrorMessages
            });
            this.$el.find(".errorTable").html(this.errorTableView.$el);
            this.renderView(this.errorTableView);

            Backbone.Hubs.ErrorHub.on("errors", this._addErrors);
            this.timer.start();
        },

        _setRange: function() {
            Backbone.Hubs.ErrorHub.off("errors", this._addErrors);
            this.$el.find(".dateRange").hide();

            var from = this._getFromTime();
            var to = moment.utc();
            this.from = from.format();
            this.to = to.format();

            var range = this.$el.find(".timeRange").val();
            if (range !== "Custom Range") {
                Backbone.Hubs.ErrorHub.on("errors", this._addErrors);
            } else {
                this.$el.find(".dateRange").show();
            }

            this.$el.find('.from').val(from.format("DD/MM/YYYY HH:mm:ss"));
            this.$el.find('.to').val(to.format("DD/MM/YYYY HH:mm:ss"));

            var that = this;
            this.collection.from = this.from;
            this.collection.to = this.to;
            this.collection.fetch({
                data: {
                    from: this.from,
                    to: this.to,
                    tags: this.tags.join()
                },
                reset: true,
                success: function() {
                    that.errorTableView.refresh();
                    that.errorHistogramView.refresh();
                }
            });
        },

        _fetchErrors: function() {
            Backbone.Hubs.ErrorHub.off("errors", this._addErrors);

            var from = moment.utc(this.$el.find('.from').val(), "DD/MM/YYYY HH:mm:ss");
            var to = moment.utc(this.$el.find('.to').val(), "DD/MM/YYYY HH:mm:ss");
            this.from = from.format();
            this.to = to.format();

            var that = this;
            this.collection.from = this.from;
            this.collection.to = this.to;
            this.collection.fetch({
                data: {
                    from: this.from,
                    to: this.to,
                    tags: this.tags.join()
                },
                reset: true,
                success: function() {
                    that.errorTableView.refresh();
                    that.errorHistogramView.refresh();
                }
            });
        },

        _addErrors: function(errors) {
            var from = this._getFromTime();
            var to = moment.utc();
            this.from = from.format();
            this.to = to.format();
            this.collection.from = this.from;
            this.collection.to = this.to;
            for (var i = 0; i < errors.length; i++) {
                this.collection.fullCollection.add(new Backbone.Model(errors[i]), {
                    at: 0
                });
            }
            this.errorTableView.addErrors(errors);
            this.errorHistogramView.refresh();
        },

        _updateCollection: function() {
            var from = this._getFromTime();
            var to = moment.utc();
            this.from = from.format();
            this.to = to.format();
            this.collection.from = this.from;
            this.collection.to = this.to;
            this.errorTableView.removeMessages(from);
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

        _retryAllMessages: function() {
            var modal = new RetryModal({
                collection: this.collection,
                onComplete: this._filterErrorMessages
            });
            $("body").append(modal.render().$el);
        },

        _retrySelectedMessages: function() {
            var data = this.errorTableView.getSelected();
            var modal = new RetryModal({
                collection: new ErrorMessagesCollection(data),
                onComplete: this._filterErrorMessages
            });
            $("body").append(modal.render().$el);
        },

        onClose: function() {
            Backbone.Hubs.ErrorHub.off("errors", this._addErrors);
            this.timer.stop();
        }
    });

    return view;
});
