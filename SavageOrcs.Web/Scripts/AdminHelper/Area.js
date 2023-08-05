var AreaView = Class.extend({
    InitializeControls: function () {
        var self = this;


        self.SubscribeEvents();
    },
    SubscribeEvents: function () {
        var self = this;

        $("#addArea").on('click', function () {
            self.Add();
        });

        $("#search").on('click', function () {
            self.Search();
        });

        $("#save").on('click', function () {
            self.Save();
        });

        $("#clear").on('click', function () {
            self.Clear();
        });

        $("#searchAll").on('click', function () {
            self.SearchAll();
        });

        $("#clearAll").on('click', function () {
            self.ClearAll();
        });
    },
    Add: function () {
        var name = $("#addName").val();
        var comunnity = $("#addCommunity").val();
        var region = $("#addRegion").val();
        if (name.length === 0 || comunnity.length === 0 || region.length === 0)
            return;

        $("#areaContainer").prepend("<div class=\"area-row pb-2\"><div class=\"area-row-first\">" +
            "<input type=\"hidden\" value=\"\"><input type=\"text\" class=\"text-box-custom form-control\" value=\"" +
            name + "\"><input type=\"text\" class=\"text-box-custom form-control\" value=\"" +
            comunnity + "\"><input type=\"text\" class=\"text-box-custom form-control\" value=\"" +
            region + "\"></div><button class=\"copy-video-button\" value=\"" + name + ", " + comunnity + ", " + region +
            "\" onclick=\"areaView.CopyAreaFullName(this)\"><img class=\"copy-video\" src=\"~/images/icons/clone.png\" /></button>" +
            "<div class=\"area-row-second\"><input type=\"text\" class=\"text-box-custom form-control\" value=\"" +
            "\"><input type=\"text\" class=\"text-box-custom form-control\" value=\"" +
            "\"><input type=\"text\" class=\"text-box-custom form-control\" value=\"" +
            "\"><button class=\"btn btn-dark-custom\" onclick=\"areaView.SaveOneRow(this)\">Зберегти</button></div></div>");
        $("#addName").val("");
        $("#addCommunity").val("");
        $("#addRegion").val("");
    },
    Search: function () {
        var filterName = $("#filterName").val().toLowerCase();
        var filterCommunity = $("#filterCommunity").val().toLowerCase();
        var filterRegion = $("#filterRegion").val().toLowerCase();

        $('#areaContainer .area-row').each(function (index, element) {
            var name = $(element).find(".text-box-custom").eq(0).val().toLowerCase();
            var community = $(element).find(".text-box-custom").eq(1).val().toLowerCase();
            var region = $(element).find(".text-box-custom").eq(2).val().toLowerCase();

            var nameEng = $(element).find(".text-box-custom").eq(3).val().toLowerCase();
            var communityEng = $(element).find(".text-box-custom").eq(4).val().toLowerCase();
            var regionEng = $(element).find(".text-box-custom").eq(5).val().toLowerCase();

            var toShowName = false;
            var toShowCommunity = false;
            var toShowRegion = false;
            if (filterName === '')
                toShowName = true;

            if (filterCommunity === '')
                toShowCommunity = true;

            if (filterRegion === '')
                toShowRegion = true;

            if (name !== '' && name.indexOf(filterName) !== -1)
                toShowName = true;

            if (nameEng !== '' && nameEng.indexOf(filterName) !== -1)
                toShowName = true;

            if (community !== '' && community.indexOf(filterCommunity) !== -1)
                toShowCommunity = true;

            if (communityEng !== '' && communityEng.indexOf(filterCommunity) !== -1)
                toShowCommunity = true;

            if (region !== '' && region.indexOf(filterRegion) !== -1)
                toShowRegion = true;

            if (regionEng !== '' && regionEng.indexOf(filterRegion) !== -1)
                toShowRegion = true;

            if (toShowName && toShowCommunity && toShowRegion)
                $(element).css({ display: 'flex' });
            else
                $(element).css({ display: 'none' });
        });

    },
    
    Clear: function () {
        $('#areaContainer .area-row').each(function (index, element) {
            $(element).css({ display: 'flex' });
        });

        $("#filterName").val("");
        $("#filterCommunity").val("");
        $("#filterRegion").val("");
    },
    ClearAll: function () {
        $("#filterNameAll").val("");
        $("#filterCommunityAll").val("");
        $("#filterRegionAll").val("");

        $(".areaSearch-result-row").empty();
    },
    SearchAll: function () {
        $(".areaSearch-result-row").empty();

        var name =  $("#filterNameAll").val();
        var community = $("#filterCommunityAll").val();
        var region = $("#filterRegionAll").val();

        var areaCatalogueViewModel = {
            Name: name,
            Community: community,
            Region: region,
        };

        $.ajax({
            type: 'POST',
            url: "/AdminHelper/SearchInAllAreas",
            data: JSON.stringify(areaCatalogueViewModel),
            contentType: 'application/json; charset=utf-8',
            success: function (src) {
                //location.reload();
                $(".areaSearch-result-row").append(src);
            }
        });
    },
    CopyAreaFullName: function (el) {
        var self = this;
        var row = $(el).parent();
        var res = row.find('input[type="text"]').eq(0).val() + ", " + row.find('input[type="text"]').eq(1).val() + ", " + row.find('input[type="text"]').eq(2).val();

        self.CopyTextToClipboard(res);
    },
    SaveOneRow: function (el) {
        var self = this;

        var mainRow = $(el).parent().parent();

        var id = mainRow.find('input[type="hidden"]').eq(0).val();

        if (id === undefined || id === null)
            id = "";

        var name = mainRow.find('input[type="text"]').eq(0).val();
        var community = mainRow.find('input[type="text"]').eq(1).val();
        var region = mainRow.find('input[type="text"]').eq(2).val();

        var nameEng = mainRow.find('input[type="text"]').eq(3).val();
        var communityEng = mainRow.find('input[type="text"]').eq(4).val();
        var regionEng = mainRow.find('input[type="text"]').eq(5).val();
        var obj = {
            Id: id,
            Name: name,
            NameEng: nameEng,
            CommunityEng: communityEng,
            Community: community,
            Region: region,
            RegionEng: regionEng,
        };
        $.ajax({
            type: 'POST',
            url: "/AdminHelper/SaveAreas",
            data: JSON.stringify([obj]),
            contentType: 'application/json; charset=utf-8',
            success: function (result) {
                location.reload();
                //ResultPopUp(result.success, result.text, result.url, result.id);
                ////mainRow.find('.copy-video-button').val(obj.Name + ", " + obj.Community + ", " + obj.Region);

                //self.Search();
                //self.SearchAll();
            }
        });

        console.log(obj);
    },
    CopyTextToClipboard: function (text) {
        if (!navigator.clipboard) {
            return;
        }
        navigator.clipboard.writeText(text);
    },
});

