using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class NetSuiteQueryRequest
  {
    public string q;

    public NetSuiteQueryRequest(string _q)
    {
      q = _q;
    }
  };

  public class NetSuiteCollection
  {
    public int count;
    public bool hasMore;
    public int offset;
    public int totalResults;
  }

  public class NetSuiteEstimateAccountBookDetailElement
  {
    public NsResource accountBook;
    public double exchangeRate;
    public NsLink[] links;
    public string refName;
    public bool revRecOnRevCommitment;
    public NetSuiteSubsidiary subsidiary;
    public bool tranIsVsoeBundle;
  }

  public class NetSuiteEstimateAccountBookDetailCollection : NetSuiteCollection
  {
    public NetSuiteEstimateAccountBookDetailElement[] items;
    public NsLink[] links;
  }

  public class NetSuiteEstimate
  {
    public string tranId { get; set; }
    public string entity { get; set; }
    public string tranDate { get; set; }

    public double probability { get; set; }
    public string expectedCloseDate { get; set; }

    public NetSuiteEntityStatus entityStatus { get; set; }

    public NetSuiteEstimateItem item { get; set; }
  }

  public class NetSuiteEntityStatus
  {
    public string id { get; set; }
    public string refName { get; set; }
  }

  public class NetSuiteEstimateItem
  {
    public List<NetSuiteItem> items { get; set; }
  }

  public class NetSuiteItem
  {
    public int line { get; set; }
    public NetSuiteLineItem item { get; set; }
    public double rate { get; set; }
    public int quantity;
  }

  public class NetSuiteLineItem
  {
    public int id { get; set; }
  }

  public class NetSuiteEstimateItemCollection
  {
    public int count;
    public bool hasMore;
    public NetSuiteEstimateItemElement[] items;
    public NsLink[] links;
    public int offset;
    public int totalResults;
  }

  public class NetSuiteEstimateItemElement { }

  public class NsLink
  {
    public string href;
    public string rel;
  }

  public class NsResource
  {
    public string externalId;
    public string id;
    public NsLink[] links;
    public string refName;
  }

  public class NetSuiteSubsidiary
  {
    public NsObject country;
    public NetSuiteCurrency currency;
    public string email;
    public string externalId;
    public string fax;
    public string fullName;
    public string id;
    public int internalId;
    public bool isElimination;
    public bool isInactive;
    public string lastModifiedDate;
    public string legalName;
    public NsLink[] links;
    public NetSuiteSubsidiaryMainAddress mainAddress;
  }

  public class NetSuiteSubsidiaryMainAddress
  {
    string addr1;
    public string addr2;
    public string addr3;
    public string addrPhone;
    public string addrText;
    public string addressee;
    public string attention;
    public string city;
    public NsObject country;
    public string externalId;
    public string lastModifiedDate;
    public NsLink[] links;
    public bool Override;
    public string refName;
    public string state;
    public string zip;
  }

  public class NsObject
  {
    public string id;
    public string refName;
  }

  public class NetSuiteCurrency
  {
    public int currencyPrecision;
    public string displaySymbol;
    public double exchangeRate;
    public string externalId;
    public string formatSample;
    public NsObject fxRateUpdateTimezone;
    public string id;
    public bool includeFxRateUpdates;
    public bool isAnchorCurrency;
    public bool isBaseCurrency;
    public bool isInactive;
    public string lastModifiedDate;
    public NsLink[] links;
    public NsObject locale;
    public string name;
    public bool overrideCurrencyFormat;
    public string refName;
    public string symbol;
    public NsObject symbolPlacement;
  }
}
